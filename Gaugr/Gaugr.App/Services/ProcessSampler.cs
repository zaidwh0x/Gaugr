using System.Diagnostics;

namespace Gaugr.App.Services;

public record ProcessInfo(string Name, int Count, double Cpu, double MemoryMb)
{
    public string CpuText => $"{Cpu:0.0}%";
    public string MemoryText => MemoryMb >= 1024 ? $"{MemoryMb / 1024:0.0} GB" : $"{MemoryMb:0} MB";
    public bool CpuHigh => Cpu >= 50;
    public bool MemoryHigh => MemoryMb >= 2048;
}

public sealed class ProcessSampler
{
    private Dictionary<int, (TimeSpan Cpu, DateTime Time)> _previous = new();
    private readonly int _cores = Environment.ProcessorCount;

    public List<ProcessInfo> Sample()
    {
        var now = DateTime.UtcNow;
        var current = new Dictionary<int, (TimeSpan Cpu, DateTime Time)>();
        var groups = new Dictionary<string, (int Count, double Cpu, double Mem)>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in Process.GetProcesses())
        {
            using (p)
            {
                if (p.Id == 0) continue;   // the "Idle" pseudo-process

                string name;
                try { name = p.ProcessName; } catch { continue; }

                double mem = 0;
                try { mem = p.WorkingSet64 / 1_048_576.0; } catch { }

                // CPU % = how much processor time was used since the last sample
                double cpu = 0;
                try
                {
                    var total = p.TotalProcessorTime;
                    current[p.Id] = (total, now);

                    if (_previous.TryGetValue(p.Id, out var prev) && (now - prev.Time).TotalSeconds < 10)
                    {
                        var elapsedMs = (now - prev.Time).TotalMilliseconds;
                        if (elapsedMs > 0)
                            cpu = (total - prev.Cpu).TotalMilliseconds / elapsedMs / _cores * 100;
                    }
                }
                catch { /* some system processes refuse access */ }

                groups.TryGetValue(name, out var g);
                groups[name] = (g.Count + 1, g.Cpu + Math.Max(cpu, 0), g.Mem + mem);
            }
        }

        _previous = current;
        return groups.Select(kv => new ProcessInfo(kv.Key, kv.Value.Count, kv.Value.Cpu, kv.Value.Mem)).ToList();
    }
}