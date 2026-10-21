using System.ComponentModel;
using System.Windows;
using Gaugr.App.Services;
using Gaugr.App.ViewModels;

namespace Gaugr.App;

public partial class App : Application
{
    public static SettingsService Settings { get; } = SettingsService.Load();
    public static MonitorHub Hub { get; } = new(Settings);
    public static TrayService Tray { get; } = new();
    public static AlertService Alerts { get; } = new(Hub, Settings, Tray);

    private MainWindow? _window;
    private OverlayWindow? _overlay;
    private bool _exiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // the tray icon, not the window, decides when Gaugr quits
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        Tray.OpenRequested += ShowWindow;
        Tray.ExitRequested += Quit;
        Tray.OverlayToggleRequested += () => Settings.SetOverlayEnabled(!Settings.OverlayEnabled);
        Settings.Changed += ApplyOverlay;

        Hub.Start();

        _window = new MainWindow();
        _window.Closing += OnWindowClosing;

        // "Start with Windows" launches with --minimized so Gaugr waits in the tray
        if (!e.Args.Contains("--minimized")) _window.Show();

        ApplyOverlay();
    }

    // Shows or hides the compact overlay to match the setting (from Settings or the tray menu)
    private void ApplyOverlay()
    {
        if (Settings.OverlayEnabled)
        {
            _overlay ??= new OverlayWindow(new OverlayViewModel(Hub, Settings), Settings);
            if (!_overlay.IsVisible) _overlay.Show();
        }
        else
        {
            _overlay?.Hide();
        }

        Tray.SetOverlayChecked(Settings.OverlayEnabled);
    }

    private void ShowWindow()
    {
        if (_window is null) return;

        _window.Show();
        if (_window.WindowState == WindowState.Minimized) _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (_exiting) return;

        if (Settings.CloseToTray)
        {
            e.Cancel = true;
            _window?.Hide();
        }
        else
        {
            Quit();
        }
    }

    private void Quit()
    {
        _exiting = true;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Tray.Dispose();
        Hub.Dispose();
        base.OnExit(e);
    }
}
