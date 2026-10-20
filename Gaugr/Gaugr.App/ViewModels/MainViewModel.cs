using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HardwareMonitorService _service = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private bool _busy;

    [ObservableProperty] private string _cpuName = "CPU";
    [ObservableProperty] private double _cpuTemp;
    [ObservableProperty] private string _gpuName = "GPU";
    [ObservableProperty] private double _gpuTemp;
    [ObservableProperty] private string _ramLabel = "RAM";
    [ObservableProperty] private double _ramPercent;
    [ObservableProperty] private string _storageLabel = "Storage";
    [ObservableProperty] private double _storagePercent;

    public MainViewModel()
    {
        _timer.Tick += async (_, _) => await RefreshAsync();
        _timer.Start();
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            var s = await Task.Run(_service.Read);   // read sensors off the UI thread

            CpuName = s.CpuName;
            CpuTemp = s.CpuTemp;
            GpuName = s.GpuName;
            GpuTemp = s.GpuTemp;
            RamLabel = s.RamLabel;
            RamPercent = s.RamPercent;
            StorageLabel = s.StorageLabel;
            StoragePercent = s.StoragePercent;
        }
        finally { _busy = false; }
    }
}