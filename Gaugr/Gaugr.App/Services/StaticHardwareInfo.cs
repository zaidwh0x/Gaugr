using System.Management;

namespace Gaugr.App.Services;

// Details that never change while the app runs, read once from Windows (WMI)
public record StaticHardwareInfo(
    string? CpuName, int Cores, int Threads, double? CpuBaseMhz,
    string? RamTotal, string? RamType, string? RamSpeed, string? RamModules,
    string? BoardMaker, string? BoardModel, string? BiosVersion)
{
    public static StaticHardwareInfo Load()
    {
        var cpu = Query("Win32_Processor", "Name", "NumberOfCores", "NumberOfLogicalProcessors", "MaxClockSpeed").FirstOrDefault();
        var mem = Query("Win32_PhysicalMemory", "Capacity", "ConfiguredClockSpeed", "Speed", "SMBIOSMemoryType");
        var board = Query("Win32_BaseBoard", "Manufacturer", "Product").FirstOrDefault();
        var bios = Query("Win32_BIOS", "SMBIOSBIOSVersion").FirstOrDefault();

        var caps = mem.Select(m => (Num(m["Capacity"]) ?? 0) / 1_073_741_824.0).Where(c => c > 0).ToList();
        double? speed = mem.Select(m => Num(m["ConfiguredClockSpeed"]) ?? Num(m["Speed"])).FirstOrDefault(v => v > 0);
        int? memType = (int?)Num(mem.FirstOrDefault()?["SMBIOSMemoryType"]);

        string? modules = caps.Count == 0 ? null
            : caps.Distinct().Count() == 1 ? $"{caps.Count} × {caps[0]:0} GB"
            : $"{caps.Count} modules";

        return new StaticHardwareInfo(
            Str(cpu?["Name"]),
            (int)(Num(cpu?["NumberOfCores"]) ?? 0),
            (int)(Num(cpu?["NumberOfLogicalProcessors"]) ?? Environment.ProcessorCount),
            Num(cpu?["MaxClockSpeed"]),
            caps.Count > 0 ? $"{caps.Sum():0} GB" : null,
            MemType(memType),
            speed is > 0 ? $"{speed:0} MT/s" : null,
            modules,
            Str(board?["Manufacturer"]),
            Str(board?["Product"]),
            Str(bios?["SMBIOSBIOSVersion"]));
    }

    private static List<Dictionary<string, object?>> Query(string className, params string[] props)
    {
        var rows = new List<Dictionary<string, object?>>();
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {string.Join(",", props)} FROM {className}");
            foreach (ManagementObject mo in searcher.Get())
            {
                using (mo)
                {
                    var row = new Dictionary<string, object?>();
                    foreach (var p in props) row[p] = mo[p];
                    rows.Add(row);
                }
            }
        }
        catch { /* if Windows refuses, we just show fewer rows */ }
        return rows;
    }

    private static string? Str(object? o) => o?.ToString()?.Trim();
    private static double? Num(object? o) => o is null ? null : Convert.ToDouble(o);

    private static string? MemType(int? t) => t switch
    {
        20 => "DDR",
        21 => "DDR2",
        24 => "DDR3",
        26 => "DDR4",
        34 => "DDR5",
        _ => null
    };
}