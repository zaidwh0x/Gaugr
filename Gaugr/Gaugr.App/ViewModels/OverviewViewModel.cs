using CommunityToolkit.Mvvm.ComponentModel;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public partial class OverviewViewModel : ObservableObject
{
    [ObservableProperty] private string _cpuName = "CPU";
    [ObservableProperty] private double _cpuTemp;
    [ObservableProperty] private string _gpuName = "GPU";
    [ObservableProperty] private double _gpuTemp;
    [ObservableProperty] private string _ramLabel = "RAM";
    [ObservableProperty] private double _ramPercent;
    [ObservableProperty] private string _storageLabel = "Storage";
    [ObservableProperty] private double _storagePercent;

    public OverviewViewModel(MonitorHub hub)
    {
        hub.Updated += Apply;
        if (hub.Latest is { } s) Apply(s);
    }

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
    }
}