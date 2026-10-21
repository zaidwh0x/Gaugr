using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public partial class OverviewViewModel : ObservableObject
{
    private readonly Action<string> _navigate;

    [ObservableProperty] private string _cpuName = "CPU";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CpuAvailable))] private double _cpuTemp;
    [ObservableProperty] private string _gpuName = "GPU";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(GpuAvailable))] private double _gpuTemp;
    [ObservableProperty] private string _ramLabel = "RAM";
    [ObservableProperty] private double _ramPercent;
    [ObservableProperty] private string _storageLabel = "Storage";
    [ObservableProperty] private double _storagePercent;
    [ObservableProperty] private double _health = 100;
    [ObservableProperty] private string _healthLabel = "Good";

    // a temperature of 0 means the sensor returned nothing (e.g. PawnIO is not installed)
    public bool CpuAvailable => CpuTemp > 0;
    public bool GpuAvailable => GpuTemp > 0;

    public string WelcomeText { get; } = $"Welcome back, {Environment.UserName}";

    public OverviewViewModel(MonitorHub hub, Action<string> navigate)
    {
        _navigate = navigate;
        hub.Updated += Apply;
        if (hub.Latest is { } s) Apply(s);
    }

    [RelayCommand]
    private void ViewMore(string page) => _navigate(page);

    private void Apply(HardwareSnapshot s)
    {
        CpuName = s.CpuName;
        CpuTemp = s.CpuTemp;
        GpuName = s.GpuName;
        GpuTemp = s.GpuTemp;
        RamLabel = s.RamLabel;
        RamPercent = s.RamPercent;
        StorageLabel = s.StorageLabel;
        StoragePercent = s.StoragePercent;

        // Simple score: the less temperature, memory and disk pressure, the healthier.
        double stress = (TempStress(s.CpuTemp) + TempStress(s.GpuTemp) + s.RamPercent + s.StoragePercent) / 4;
        Health = Math.Round(Math.Clamp(100 - stress, 0, 100));
        HealthLabel = Health >= 75 ? "Good" : Health >= 50 ? "Fair" : "Poor";
    }

    // 30 °C counts as no stress, 100 °C as maximum
    private static double TempStress(double celsius) => Math.Clamp((celsius - 30) / 70 * 100, 0, 100);
}
