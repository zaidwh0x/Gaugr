using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public record PlaceholderViewModel(string Title);

public partial class ShellViewModel : ObservableObject
{
    private readonly OverviewViewModel _overview;

    [ObservableProperty] private object _currentPage;

    public ShellViewModel(MonitorHub hub)
    {
        _overview = new OverviewViewModel(hub);
        _currentPage = _overview;
    }

    [RelayCommand]
    private void Navigate(string name)
    {
        CurrentPage = name == "Overview" ? _overview : new PlaceholderViewModel(name);
    }
}