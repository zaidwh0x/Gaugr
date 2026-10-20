using LibreHardwareMonitor.Hardware;
using System.IO;

namespace Gaugr.App.Services;

public record HardwareSnapshot(
    string CpuName, double CpuTemp,
    string GpuName, double GpuTemp,
    double RamPercent, string RamLabel,
    double StoragePercent, string StorageLabel);

public sealed class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer = new()
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true
    };

    public HardwareMonitorService() => _computer.Open();

    public HardwareSnapshot Read()
    {
        string cpuName = "CPU", gpuName = "GPU";
        double cpuTemp = 0, gpuTemp = 0, ramPercent = 0, ramUsed = 0, ramAvail = 0;

        foreach (var hw in _computer.Hardware)
        {
            hw.Update();

            switch (hw.HardwareType)
            {
                case HardwareType.Cpu:
                    cpuName = hw.Name;
                    cpuTemp = Get(hw, SensorType.Temperature, "Tctl", "Package", "Average", "Core");
                    break;

                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    // Prefer an NVIDIA card; otherwise keep the first GPU found
                    // (this skips the CPU's built-in graphics on most gaming PCs)
                    if (hw.HardwareType == HardwareType.GpuNvidia || gpuName == "GPU")
                    {
                        gpuName = hw.Name;
                        gpuTemp = Get(hw, SensorType.Temperature, "GPU Core");
                    }
                    break;

                case HardwareType.Memory:
                    ramPercent = Get(hw, SensorType.Load, "Memory");
                    ramUsed = Get(hw, SensorType.Data, "Memory Used");
                    ramAvail = Get(hw, SensorType.Data, "Memory Available");
                    break;
            }
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

        return new HardwareSnapshot(cpuName, cpuTemp, gpuName, gpuTemp,
                                    ramPercent, ramLabel, storagePercent, storageLabel);
    }

    // Finds a sensor by name hint (exact match first, then "contains").
    // Sensor names differ between CPUs/GPUs, so we try several hints.
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