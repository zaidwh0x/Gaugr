using CommunityToolkit.Mvvm.ComponentModel;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public record HardwareRow(string Label, string Value);

public partial class HardwareSection : ObservableObject
{
    public string Title { get; }

    [ObservableProperty] private IReadOnlyList<HardwareRow> _rows = Array.Empty<HardwareRow>();
    [ObservableProperty] private bool _hasRows;

    public HardwareSection(string title) => Title = title;

    partial void OnRowsChanged(IReadOnlyList<HardwareRow> value) => HasRows = value.Count > 0;
}

public partial class HardwareViewModel : ObservableObject
{
    private readonly MonitorHub _hub;
    private StaticHardwareInfo? _static;

    public HardwareSection Cpu { get; } = new("Processor");
    public HardwareSection Gpu { get; } = new("Graphics");
    public HardwareSection Memory { get; } = new("Memory");
    public HardwareSection Board { get; } = new("Motherboard");
    public HardwareSection Fans { get; } = new("Fans");

    public IReadOnlyList<HardwareSection> Sections { get; }

    public HardwareViewModel(MonitorHub hub)
    {
        _hub = hub;
        Sections = new[] { Cpu, Gpu, Memory, Board, Fans };

        hub.Updated += _ => Rebuild();
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _static = await Task.Run(StaticHardwareInfo.Load);
        Rebuild();
    }

    private void Rebuild()
    {
        var st = _static;
        var s = _hub.Latest;

        // Processor
        var cpu = new List<HardwareRow>();
        Add(cpu, "Processor", st?.CpuName ?? s?.CpuName);
        if (st is { Cores: > 0 }) Add(cpu, "Cores / threads", $"{st.Cores} cores · {st.Threads} threads");
        if (st?.CpuBaseMhz is double baseMhz and > 0) Add(cpu, "Base clock", Ghz(baseMhz));
        if (s?.CpuClockMhz is double clock) Add(cpu, "Current clock", Ghz(clock));
        Cpu.Rows = cpu;

        // Graphics
        var gpu = new List<HardwareRow>();
        if (s is not null && s.GpuName != "GPU") Add(gpu, "Graphics card", s.GpuName);
        if (s?.VramTotalMb is double total and > 0)
        {
            string vram = s.VramUsedMb is double used
                ? $"{used / 1024:0.0} / {total / 1024:0.0} GB"
                : $"{total / 1024:0.0} GB";
            Add(gpu, "Video memory", vram);
        }
        if (s?.GpuCoreClockMhz is double core) Add(gpu, "Core clock", $"{core:0} MHz");
        if (s?.GpuMemoryClockMhz is double mem) Add(gpu, "Memory clock", $"{mem:0} MHz");
        Gpu.Rows = gpu;

        // Memory
        var ram = new List<HardwareRow>();
        Add(ram, "Total", st?.RamTotal);
        Add(ram, "Type", st?.RamType);
        Add(ram, "Speed", st?.RamSpeed);
        Add(ram, "Modules", st?.RamModules);
        Memory.Rows = ram;

        // Motherboard
        var board = new List<HardwareRow>();
        Add(board, "Manufacturer", st?.BoardMaker);
        Add(board, "Model", st?.BoardModel);
        Add(board, "BIOS version", st?.BiosVersion);
        Board.Rows = board;

        // Fans (only those that report a speed)
        var fans = new List<HardwareRow>();
        if (s is not null)
            foreach (var f in s.Fans) fans.Add(new HardwareRow(f.Name, $"{f.Rpm:0} RPM"));
        Fans.Rows = fans;
    }

    private static void Add(List<HardwareRow> rows, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) rows.Add(new HardwareRow(label, value));
    }

    private static string Ghz(double mhz) => $"{mhz / 1000:0.00} GHz";
}