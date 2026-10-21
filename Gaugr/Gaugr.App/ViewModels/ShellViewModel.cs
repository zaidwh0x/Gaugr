using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public record PlaceholderViewModel(string Title);

public partial class NavItem : ObservableObject
{
    public NavItem(string name, string glyph)
    {
        Name = name;
        Glyph = glyph;
    }

    public string Name { get; }
    public string Glyph { get; }

    [ObservableProperty] private bool _isSelected;
}

public partial class ShellViewModel : ObservableObject
{
    private readonly OverviewViewModel _overview;
    private readonly PerformanceViewModel _performance;
    private readonly HardwareViewModel _hardware;
    private readonly ProcessesViewModel _processes;

    [ObservableProperty] private object _currentPage;

    public IReadOnlyList<NavItem> NavItems { get; } =
    [
        new("Overview", ""),
        new("Performance", ""),
        new("Hardware", ""),
        new("Processes", ""),
        new("Storage", ""),
        new("Settings", ""),
    ];

    public ShellViewModel(MonitorHub hub)
    {
        _overview = new OverviewViewModel(hub, Navigate);
        _performance = new PerformanceViewModel(hub);
        _hardware = new HardwareViewModel(hub);
        _processes = new ProcessesViewModel();
        _currentPage = _overview;
        NavItems[0].IsSelected = true;
    }

    [RelayCommand]
    private void Navigate(string name)
    {
        _processes.SetActive(name == "Processes");

        foreach (var item in NavItems)
            item.IsSelected = item.Name == name;

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
