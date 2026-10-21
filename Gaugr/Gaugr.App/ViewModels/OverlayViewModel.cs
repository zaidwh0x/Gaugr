using CommunityToolkit.Mvvm.ComponentModel;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public partial class OverlayViewModel : ObservableObject
{
    private readonly SettingsService _settings;

    [ObservableProperty] private string _cpuValue = "--";
    [ObservableProperty] private string _gpuValue = "--";
    [ObservableProperty] private string _ramValue = "--";

    public OverlayViewModel(MonitorHub hub, SettingsService settings)
    {
        _settings = settings;

        hub.Updated += Apply;
        settings.Changed += () =>
        {
            if (hub.Latest is { } latest) Apply(latest);
        };
        if (hub.Latest is { } s) Apply(s);
    }

    private void Apply(HardwareSnapshot s)
    {
        CpuValue = Format(s.CpuTemp, s.CpuLoad);
        GpuValue = Format(s.GpuTemp, s.GpuLoad);
        RamValue = $"{s.RamPercent:0}%";
    }

    // a temperature of 0 means the sensor returned nothing, so only the load is shown
    private string Format(double celsius, double load) => celsius > 0
        ? $"{_settings.ConvertTemperature(celsius):0}{_settings.TemperatureLabel}  ·  {load:0}%"
        : $"{load:0}%";
}
