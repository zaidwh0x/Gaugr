using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gaugr.App.Services;

namespace Gaugr.App.ViewModels;

/// <summary>One alert limit with − / + buttons. A value of 0 means the alert is off.</summary>
public partial class AlertThreshold : ObservableObject
{
    private readonly string _unit;
    private readonly int _min, _max, _step;
    private readonly Action<int> _save;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(Text))] private int _value;

    public AlertThreshold(string title, string unit, int min, int max, int step, int value, Action<int> save)
    {
        Title = title;
        _unit = unit;
        _min = min;
        _max = max;
        _step = step;
        _value = value;
        _save = save;
    }

    public string Title { get; }
    public string Text => Value == 0 ? "Off" : $"{Value} {_unit}";

    [RelayCommand]
    private void Increase()
    {
        Value = Value == 0 ? _min : Math.Min(Value + _step, _max);
        _save(Value);
    }

    [RelayCommand]
    private void Decrease()
    {
        Value = Value <= _min ? 0 : Value - _step;
        _save(Value);
    }
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly StartupService _startup;
    private readonly Action _sendTestAlert;
    private bool _startWithWindows;

    [ObservableProperty] private string _startupMessage = "";

    public SettingsViewModel(SettingsService settings, StartupService startup, Action sendTestAlert)
    {
        _settings = settings;
        _startup = startup;
        _sendTestAlert = sendTestAlert;

        Thresholds =
        [
            Threshold("CPU temperature", "°C", 60, 100, AlertMetric.CpuTemperature),
            Threshold("GPU temperature", "°C", 60, 100, AlertMetric.GpuTemperature),
            Threshold("Memory usage", "%", 50, 100, AlertMetric.Ram),
            Threshold("Storage usage", "%", 50, 100, AlertMetric.Storage),
        ];
    }

    public IReadOnlyList<AlertThreshold> Thresholds { get; }

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

    public bool CloseToTray
    {
        get => _settings.CloseToTray;
        set => _settings.SetCloseToTray(value);
    }

    public bool AlertsEnabled
    {
        get => _settings.AlertsEnabled;
        set => _settings.SetAlertsEnabled(value);
    }

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

    [RelayCommand]
    private void SendTestAlert() => _sendTestAlert();

    // Called each time the page opens, since the startup task can be changed outside Gaugr
    public void Refresh()
    {
        _startWithWindows = _startup.IsEnabled();
        OnPropertyChanged(nameof(StartWithWindows));
        StartupMessage = "";
    }

    private AlertThreshold Threshold(string title, string unit, int min, int max, AlertMetric metric) =>
        new(title, unit, min, max, 5, _settings.GetAlertLimit(metric), value => _settings.SetAlertLimit(metric, value));

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
