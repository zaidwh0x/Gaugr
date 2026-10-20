using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public record PlaceholderViewModel(string Title);

public partial class ShellViewModel : ObservableObject
{
    private readonly OverviewViewModel _overview;
    private readonly PerformanceViewModel _performance;

    [ObservableProperty] private object _currentPage;

    public ShellViewModel(MonitorHub hub)
    {
        _overview = new OverviewViewModel(hub);
        _performance = new PerformanceViewModel(hub);
        _currentPage = _overview;
    }

    [RelayCommand]
    private void Navigate(string name)
    {
        CurrentPage = name switch
        {
            "Overview" => (object)_overview,
            "Performance" => _performance,
            _ => new PlaceholderViewModel(name)
        };
    }
}