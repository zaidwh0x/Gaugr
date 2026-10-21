using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gaugr.App.Services;

public enum TemperatureUnit { Celsius, Fahrenheit }

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

    public TemperatureUnit TemperatureUnit { get; private set; } = TemperatureUnit.Celsius;
    public double RefreshSeconds { get; private set; } = 1;

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
                if (data is not null)
                {
                    settings.TemperatureUnit = data.TemperatureUnit;
                    if (RefreshChoices.Contains(data.RefreshSeconds)) settings.RefreshSeconds = data.RefreshSeconds;
                }
            }
        }
        catch { /* a missing or damaged settings file just means defaults */ }

        return settings;
    }

    public double ConvertTemperature(double celsius) =>
        TemperatureUnit == TemperatureUnit.Fahrenheit ? celsius * 9 / 5 + 32 : celsius;

    public void SetTemperatureUnit(TemperatureUnit unit)
    {
        if (unit == TemperatureUnit) return;
        TemperatureUnit = unit;
        SaveAndNotify();
    }

    public void SetRefreshSeconds(double seconds)
    {
        if (!RefreshChoices.Contains(seconds) || seconds == RefreshSeconds) return;
        RefreshSeconds = seconds;
        SaveAndNotify();
    }

    private void SaveAndNotify()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(new SettingsData(TemperatureUnit, RefreshSeconds), JsonOptions));
        }
        catch { /* the setting still applies for this session */ }

        Changed?.Invoke();
    }

    internal sealed record SettingsData(TemperatureUnit TemperatureUnit, double RefreshSeconds);
}
