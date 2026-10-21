using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gaugr.App.Services;
using SkiaSharp;

namespace Gaugr.App.ViewModels;

public partial class PerformanceViewModel : ObservableObject
{
    private readonly MonitorHub _hub;
    private readonly SettingsService _settings;
    private readonly ObservableCollection<double> _cpuLoad = new(), _cpuTemp = new(),
                                                  _gpuLoad = new(), _gpuTemp = new(),
                                                  _ramUsed = new();

    [ObservableProperty] private int _windowSeconds = 60;

    public ChartModel Cpu { get; }
    public ChartModel Gpu { get; }
    public ChartModel Ram { get; }

    // How many samples fit in the chosen window at the current refresh rate
    private int Samples => Math.Max(2, (int)Math.Round(WindowSeconds / _hub.IntervalSeconds));

    public PerformanceViewModel(MonitorHub hub, SettingsService settings)
    {
        _hub = hub;
        _settings = settings;

        var blue = new SKColor(40, 140, 255);
        var orange = new SKColor(255, 160, 40);
        var green = new SKColor(52, 211, 153);

        Cpu = new ChartModel((_cpuLoad, blue), (_cpuTemp, orange));
        Gpu = new ChartModel((_gpuLoad, blue), (_gpuTemp, orange));
        Ram = new ChartModel((_ramUsed, green));

        ApplyWindow();
        hub.Updated += OnUpdated;
        hub.IntervalChanged += ApplyWindow;
        settings.Changed += () =>
        {
            if (_hub.Latest is { } latest) UpdateSummaries(latest);
        };
    }

    partial void OnWindowSecondsChanged(int value) => ApplyWindow();

    [RelayCommand]
    private void SetWindow(string seconds) => WindowSeconds = int.Parse(seconds);

    private void ApplyWindow()
    {
        foreach (var chart in new[] { Cpu, Gpu, Ram }) chart.SetWindow(Samples);

        Refill(_cpuLoad, s => s.CpuLoad);
        Refill(_cpuTemp, s => s.CpuTemp);
        Refill(_gpuLoad, s => s.GpuLoad);
        Refill(_gpuTemp, s => s.GpuTemp);
        Refill(_ramUsed, s => s.RamPercent);

        if (_hub.Latest is { } latest) UpdateSummaries(latest);
    }

    // Rebuilds a graph from the shared history (used when you switch 60 s / 5 min)
    private void Refill(ObservableCollection<double> target, Func<HardwareSnapshot, double> pick)
    {
        target.Clear();
        foreach (var s in _hub.History.TakeLast(Samples)) target.Add(pick(s));
    }

    private void OnUpdated(HardwareSnapshot s)
    {
        Push(_cpuLoad, s.CpuLoad);
        Push(_cpuTemp, s.CpuTemp);
        Push(_gpuLoad, s.GpuLoad);
        Push(_gpuTemp, s.GpuTemp);
        Push(_ramUsed, s.RamPercent);
        UpdateSummaries(s);
    }

    private void Push(ObservableCollection<double> list, double value)
    {
        list.Add(value);
        while (list.Count > Samples) list.RemoveAt(0);
    }

    // Charts always plot temperature in °C; only the summary text follows the chosen unit
    private void UpdateSummaries(HardwareSnapshot s)
    {
        var unit = _settings.TemperatureLabel;
        Cpu.Summary = $"{s.CpuLoad:0}% usage  ·  {_settings.ConvertTemperature(s.CpuTemp):0}{unit}";
        Gpu.Summary = $"{s.GpuLoad:0}% usage  ·  {_settings.ConvertTemperature(s.GpuTemp):0}{unit}";
        Ram.Summary = $"{s.RamPercent:0}%  ·  {s.RamLabel}";
    }
}
