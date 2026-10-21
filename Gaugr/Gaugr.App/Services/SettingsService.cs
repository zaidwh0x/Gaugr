using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gaugr.App.Services;

public enum TemperatureUnit { Celsius, Fahrenheit }

public enum AlertMetric { CpuTemperature, GpuTemperature, Ram, Storage }

/// <summary>User preferences, stored as JSON in %AppData%\Gaugr\settings.json.</summary>
public sealed class SettingsService
{
    public static readonly double[] RefreshChoices = { 0.5, 1, 2, 5 };

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaugr", "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private SettingsData _data = new();

    public TemperatureUnit TemperatureUnit => _data.TemperatureUnit;
    public double RefreshSeconds => _data.RefreshSeconds;
    public bool CloseToTray => _data.CloseToTray;
    public bool AlertsEnabled => _data.AlertsEnabled;
    public bool OverlayEnabled => _data.OverlayEnabled;
    public double? OverlayLeft => _data.OverlayLeft;
    public double? OverlayTop => _data.OverlayTop;

    public string TemperatureLabel => TemperatureUnit == TemperatureUnit.Fahrenheit ? "°F" : "°C";

    public event Action? Changed;

    public static SettingsService Load()
    {
        var settings = new SettingsService();
        try
        {
            if (File.Exists(FilePath))
            {
                var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(FilePath), JsonOptions);
                if (data is not null) settings._data = Sanitize(data);
            }
        }
        catch { /* a missing or damaged settings file just means defaults */ }

        return settings;
    }

    public double ConvertTemperature(double celsius) =>
        TemperatureUnit == TemperatureUnit.Fahrenheit ? celsius * 9 / 5 + 32 : celsius;

    /// <summary>Alert limit for a metric: °C for temperatures, % for usage. 0 means the alert is off.</summary>
    public int GetAlertLimit(AlertMetric metric) => metric switch
    {
        AlertMetric.CpuTemperature => _data.CpuAlert,
        AlertMetric.GpuTemperature => _data.GpuAlert,
        AlertMetric.Ram => _data.RamAlert,
        _ => _data.StorageAlert
    };

    public void SetTemperatureUnit(TemperatureUnit unit) => Update(d => d.TemperatureUnit = unit);

    public void SetRefreshSeconds(double seconds)
    {
        if (RefreshChoices.Contains(seconds)) Update(d => d.RefreshSeconds = seconds);
    }

    public void SetCloseToTray(bool value) => Update(d => d.CloseToTray = value);

    public void SetAlertsEnabled(bool value) => Update(d => d.AlertsEnabled = value);

    public void SetOverlayEnabled(bool value) => Update(d => d.OverlayEnabled = value);

    // Saved while dragging the overlay, so nothing else needs to hear about it
    public void SetOverlayPosition(double left, double top) => Update(d =>
    {
        d.OverlayLeft = left;
        d.OverlayTop = top;
    }, notify: false);

    public void SetAlertLimit(AlertMetric metric, int limit) => Update(d =>
    {
        limit = Math.Clamp(limit, 0, 100);
        switch (metric)
        {
            case AlertMetric.CpuTemperature: d.CpuAlert = limit; break;
            case AlertMetric.GpuTemperature: d.GpuAlert = limit; break;
            case AlertMetric.Ram: d.RamAlert = limit; break;
            default: d.StorageAlert = limit; break;
        }
    });

    private void Update(Action<SettingsData> change, bool notify = true)
    {
        change(_data);
        Save();
        if (notify) Changed?.Invoke();
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(_data, JsonOptions));
        }
        catch { /* the setting still applies for this session */ }
    }

    private static SettingsData Sanitize(SettingsData data)
    {
        if (!RefreshChoices.Contains(data.RefreshSeconds)) data.RefreshSeconds = 1;
        data.CpuAlert = Math.Clamp(data.CpuAlert, 0, 100);
        data.GpuAlert = Math.Clamp(data.GpuAlert, 0, 100);
        data.RamAlert = Math.Clamp(data.RamAlert, 0, 100);
        data.StorageAlert = Math.Clamp(data.StorageAlert, 0, 100);
        return data;
    }

    internal sealed class SettingsData
    {
        public TemperatureUnit TemperatureUnit { get; set; } = TemperatureUnit.Celsius;
        public double RefreshSeconds { get; set; } = 1;
        public bool CloseToTray { get; set; }
        public bool AlertsEnabled { get; set; } = true;
        public int CpuAlert { get; set; } = 90;
        public int GpuAlert { get; set; } = 90;
        public int RamAlert { get; set; } = 95;
        public int StorageAlert { get; set; }
        public bool OverlayEnabled { get; set; }
        public double? OverlayLeft { get; set; }
        public double? OverlayTop { get; set; }
    }
}
