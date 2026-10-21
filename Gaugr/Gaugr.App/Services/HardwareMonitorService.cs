using LibreHardwareMonitor.Hardware;
using System.IO;

namespace Gaugr.App.Services;

public record FanReading(string Name, double Rpm);

public record HardwareSnapshot(
    string CpuName, double CpuTemp, double CpuLoad, double? CpuClockMhz,
    string GpuName, double GpuTemp, double GpuLoad,
    double? GpuCoreClockMhz, double? GpuMemoryClockMhz, double? VramUsedMb, double? VramTotalMb,
    double RamPercent, string RamLabel,
    double StoragePercent, string StorageLabel,
    IReadOnlyList<FanReading> Fans);

public sealed class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer = new()
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true,
        IsMotherboardEnabled = true
    };

    public HardwareMonitorService() => _computer.Open();

    public HardwareSnapshot Read()
    {
        string cpuName = "CPU", gpuName = "GPU";
        double cpuTemp = 0, cpuLoad = 0, gpuTemp = 0, gpuLoad = 0;
        double ramPercent = 0, ramUsed = 0, ramAvail = 0;
        double? cpuClock = null, gpuCore = null, gpuMem = null, vramUsed = null, vramTotal = null;
        var fans = new List<FanReading>();

        foreach (var hw in _computer.Hardware)
        {
            hw.Update();

            switch (hw.HardwareType)
            {
                case HardwareType.Cpu:
                    cpuName = hw.Name;
                    cpuTemp = Get(hw, SensorType.Temperature, "Tctl", "Package", "Average", "Core");
                    cpuLoad = Get(hw, SensorType.Load, "CPU Total");

                    var clocks = hw.Sensors
                        .Where(s => s.SensorType == SensorType.Clock && s.Value > 0
                                    && s.Name.StartsWith("Core", StringComparison.OrdinalIgnoreCase)
                                    && !s.Name.Contains("Effective", StringComparison.OrdinalIgnoreCase))
                        .Select(s => (double)s.Value!.Value)
                        .ToList();
                    cpuClock = clocks.Count > 0 ? clocks.Average() : null;
                    break;

                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    if (hw.HardwareType == HardwareType.GpuNvidia || gpuName == "GPU")
                    {
                        gpuName = hw.Name;
                        gpuTemp = Get(hw, SensorType.Temperature, "GPU Core");
                        gpuLoad = Get(hw, SensorType.Load, "GPU Core");
                        gpuCore = GetOrNull(hw, SensorType.Clock, "GPU Core");
                        gpuMem = GetOrNull(hw, SensorType.Clock, "GPU Memory");
                        vramUsed = GetOrNull(hw, SensorType.SmallData, "GPU Memory Used");
                        vramTotal = GetOrNull(hw, SensorType.SmallData, "GPU Memory Total");
                    }
                    break;

                case HardwareType.Memory:
                    // With the PawnIO driver installed, individual DIMMs can also appear as Memory
                    // hardware; only the total-memory entry has the "Memory" load sensor.
                    var memoryLoad = GetOrNull(hw, SensorType.Load, "Memory");
                    if (memoryLoad is null) break;

                    ramPercent = memoryLoad.Value;
                    ramUsed = Get(hw, SensorType.Data, "Memory Used");
                    ramAvail = Get(hw, SensorType.Data, "Memory Available");
                    break;
            }

            CollectFans(hw, fans);
        }

        double storagePercent = 0;
        string storageLabel = "Storage";
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)!);
            double total = drive.TotalSize;
            double free = drive.AvailableFreeSpace;
            storagePercent = (total - free) / total * 100;
            storageLabel = $"{drive.Name.TrimEnd('\\')}  {free / 1_073_741_824.0:0} GB free";
        }
        catch { /* leave defaults */ }

        string ramLabel = $"{ramUsed:0.0} / {ramUsed + ramAvail:0.0} GB";

        return new HardwareSnapshot(
            cpuName, cpuTemp, cpuLoad, cpuClock,
            gpuName, gpuTemp, gpuLoad, gpuCore, gpuMem, vramUsed, vramTotal,
            ramPercent, ramLabel, storagePercent, storageLabel, fans);
    }

    // Fan sensors usually live on the motherboard's sub-hardware (the "Super I/O" chip)
    private static void CollectFans(IHardware hw, List<FanReading> fans)
    {
        foreach (var s in hw.Sensors)
            if (s.SensorType == SensorType.Fan && s.Value > 0)
                fans.Add(new FanReading(s.Name, s.Value!.Value));

        foreach (var sub in hw.SubHardware)
        {
            sub.Update();
            CollectFans(sub, fans);
        }
    }

    // Same as Get, but returns null when the sensor doesn't exist (so the page can hide the row)
    private static double? GetOrNull(IHardware hw, SensorType type, string hint)
    {
        var sensors = hw.Sensors.Where(s => s.SensorType == type && s.Value.HasValue).ToList();
        var match =
            sensors.FirstOrDefault(s => s.Name.Equals(hint, StringComparison.OrdinalIgnoreCase)) ??
            sensors.FirstOrDefault(s => s.Name.Contains(hint, StringComparison.OrdinalIgnoreCase));
        return match?.Value;
    }

    private static double Get(IHardware hw, SensorType type, params string[] hints)
    {
        var sensors = hw.Sensors.Where(s => s.SensorType == type && s.Value.HasValue).ToList();

        foreach (var hint in hints)
        {
            var match =
                sensors.FirstOrDefault(s => s.Name.Equals(hint, StringComparison.OrdinalIgnoreCase)) ??
                sensors.FirstOrDefault(s => s.Name.Contains(hint, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match.Value!.Value;
        }
        return sensors.FirstOrDefault()?.Value ?? 0;
    }

    public void Dispose() => _computer.Close();
}