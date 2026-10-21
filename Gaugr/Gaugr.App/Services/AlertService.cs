namespace Gaugr.App.Services;

/// <summary>
/// Raises a tray notification when a reading crosses its limit. Each alert fires once, and is
/// armed again only after the reading has dropped a few points below the limit.
/// </summary>
public sealed class AlertService
{
    private const double RearmMargin = 3;

    private readonly SettingsService _settings;
    private readonly TrayService _tray;
    private readonly HashSet<AlertMetric> _active = new();

    public AlertService(MonitorHub hub, SettingsService settings, TrayService tray)
    {
        _settings = settings;
        _tray = tray;
        hub.Updated += Check;
    }

    public void SendTest() => _tray.Notify("Gaugr alerts are working", "You will see a message like this when a limit is crossed.");

    private void Check(HardwareSnapshot s)
    {
        if (!_settings.AlertsEnabled)
        {
            _active.Clear();
            return;
        }

        Evaluate(AlertMetric.CpuTemperature, "CPU temperature", s.CpuTemp, "°C");
        Evaluate(AlertMetric.GpuTemperature, "GPU temperature", s.GpuTemp, "°C");
        Evaluate(AlertMetric.Ram, "Memory usage", s.RamPercent, "%");
        Evaluate(AlertMetric.Storage, "Storage usage", s.StoragePercent, "%");
    }

    private void Evaluate(AlertMetric metric, string label, double value, string unit)
    {
        int limit = _settings.GetAlertLimit(metric);

        // off, or no reading (a temperature of 0 means the sensor returned nothing)
        if (limit <= 0 || value <= 0)
        {
            _active.Remove(metric);
            return;
        }

        if (value >= limit)
        {
            if (_active.Add(metric))
                _tray.Notify($"{label} is high", $"{value:0}{unit} (your limit is {limit}{unit})");
        }
        else if (value < limit - RearmMargin)
        {
            _active.Remove(metric);
        }
    }
}
