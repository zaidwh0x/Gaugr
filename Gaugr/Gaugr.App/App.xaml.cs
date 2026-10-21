using System.Windows;
using Gaugr.App.Services;

namespace Gaugr.App;

public partial class App : Application
{
    public static SettingsService Settings { get; } = SettingsService.Load();
    public static MonitorHub Hub { get; } = new(Settings);

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Hub.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Hub.Dispose();
        base.OnExit(e);
    }
}
