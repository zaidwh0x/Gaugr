using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public record PlaceholderViewModel(string Title);

public partial class ShellViewModel : ObservableObject
{
    private readonly OverviewViewModel _overview;
    private readonly PerformanceViewModel _performance;
    private readonly HardwareViewModel _hardware;
    private readonly ProcessesViewModel _processes;

    [ObservableProperty] private object _currentPage;

    public ShellViewModel(MonitorHub hub)
    {
        _overview = new OverviewViewModel(hub);
        _performance = new PerformanceViewModel(hub);
        _hardware = new HardwareViewModel(hub);
        _processes = new ProcessesViewModel();
        _currentPage = _overview;
    }

    [RelayCommand]
    private void Navigate(string name)
    {
        _processes.SetActive(name == "Processes");

        CurrentPage = name switch
        {
            "Overview" => (object)_overview,
            "Performance" => _performance,
            "Hardware" => _hardware,
            "Processes" => _processes,
            _ => new PlaceholderViewModel(name)
        };
    }
}