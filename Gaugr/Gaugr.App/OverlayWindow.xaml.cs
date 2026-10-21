using System.Windows;
using System.Windows.Input;
using Gaugr.App.Services;
using Gaugr.App.ViewModels;

namespace Gaugr.App;

public partial class OverlayWindow : Window
{
    private const double ScreenMargin = 24;

    private readonly SettingsService _settings;

    public OverlayWindow(OverlayViewModel viewModel, SettingsService settings)
    {
        InitializeComponent();
        DataContext = viewModel;
        _settings = settings;
        Loaded += (_, _) => Place();
    }

    // Back where it was last time, or the top-right corner if that spot is no longer on a screen
    private void Place()
    {
        if (_settings.OverlayLeft is { } left && _settings.OverlayTop is { } top && IsOnScreen(left, top))
        {
            Left = left;
            Top = top;
            return;
        }

        var area = SystemParameters.WorkArea;
        Left = area.Right - ActualWidth - ScreenMargin;
        Top = area.Top + ScreenMargin;
    }

    private bool IsOnScreen(double left, double top) =>
        left + ActualWidth > SystemParameters.VirtualScreenLeft + 40 &&
        left < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 40 &&
        top + ActualHeight > SystemParameters.VirtualScreenTop + 40 &&
        top < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 40;

    private void OnDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed) return;

        DragMove();   // blocks until the button is released, and no mouse-up event follows
        _settings.SetOverlayPosition(Left, Top);
    }

    private void OnClose(object sender, RoutedEventArgs e) => _settings.SetOverlayEnabled(false);
}
