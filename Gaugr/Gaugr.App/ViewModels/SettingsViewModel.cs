using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly StartupService _startup;
    private bool _startWithWindows;

    [ObservableProperty] private string _startupMessage = "";

    public SettingsViewModel(SettingsService settings, StartupService startup)
    {
        _settings = settings;
        _startup = startup;
    }

    // Radio buttons bind two-way to these; only the one being switched on does anything.
    public bool IsCelsius
    {
        get => _settings.TemperatureUnit == TemperatureUnit.Celsius;
        set { if (value) SetUnit(TemperatureUnit.Celsius); }
    }

    public bool IsFahrenheit
    {
        get => _settings.TemperatureUnit == TemperatureUnit.Fahrenheit;
        set { if (value) SetUnit(TemperatureUnit.Fahrenheit); }
    }

    public bool IsRefreshHalfSecond { get => _settings.RefreshSeconds == 0.5; set { if (value) SetRefresh(0.5); } }
    public bool IsRefreshOneSecond { get => _settings.RefreshSeconds == 1; set { if (value) SetRefresh(1); } }
    public bool IsRefreshTwoSeconds { get => _settings.RefreshSeconds == 2; set { if (value) SetRefresh(2); } }
    public bool IsRefreshFiveSeconds { get => _settings.RefreshSeconds == 5; set { if (value) SetRefresh(5); } }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set
        {
            if (_startWithWindows == value) return;

            bool ok = value ? _startup.Enable() : _startup.Disable();
            if (ok)
            {
                _startWithWindows = value;
                StartupMessage = "";
            }
            else
            {
                StartupMessage = "Windows did not allow that change. Make sure Gaugr is running as administrator and try again.";
            }

            // Re-read after the binding finishes writing, so a refused change flips the switch back
            Application.Current.Dispatcher.BeginInvoke(() => OnPropertyChanged(nameof(StartWithWindows)));
        }
    }

    // Called each time the page opens, since the startup task can be changed outside Gaugr
    public void Refresh()
    {
        _startWithWindows = _startup.IsEnabled();
        OnPropertyChanged(nameof(StartWithWindows));
        StartupMessage = "";
    }

    private void SetUnit(TemperatureUnit unit)
    {
        _settings.SetTemperatureUnit(unit);
        OnPropertyChanged(nameof(IsCelsius));
        OnPropertyChanged(nameof(IsFahrenheit));
    }

    private void SetRefresh(double seconds)
    {
        _settings.SetRefreshSeconds(seconds);
        OnPropertyChanged(nameof(IsRefreshHalfSecond));
        OnPropertyChanged(nameof(IsRefreshOneSecond));
        OnPropertyChanged(nameof(IsRefreshTwoSeconds));
        OnPropertyChanged(nameof(IsRefreshFiveSeconds));
    }
}
