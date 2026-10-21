using System.Windows.Threading;

namespace Gaugr.App.Services;

public sealed class MonitorHub : IDisposable
{
    public const int HistoryLength = 600;   // 5 minutes at the fastest refresh rate (2 samples per second)

    private readonly SettingsService _settings;
    private readonly HardwareMonitorService _hardware = new();
    private readonly DispatcherTimer _timer = new();
    private readonly List<HardwareSnapshot> _history = new();
    private bool _busy;

    public MonitorHub(SettingsService settings)
    {
        _settings = settings;
        _timer.Interval = TimeSpan.FromSeconds(settings.RefreshSeconds);
        settings.Changed += OnSettingsChanged;
    }

    public HardwareSnapshot? Latest { get; private set; }
    public IReadOnlyList<HardwareSnapshot> History => _history;
    public double IntervalSeconds => _timer.Interval.TotalSeconds;

    public event Action<HardwareSnapshot>? Updated;
    public event Action? IntervalChanged;

    public void Start()
    {
        _timer.Tick += async (_, _) => await TickAsync();
        _timer.Start();
        _ = TickAsync();
    }

    private void OnSettingsChanged()
    {
        var interval = TimeSpan.FromSeconds(_settings.RefreshSeconds);
        if (interval == _timer.Interval) return;

        _timer.Interval = interval;
        _history.Clear();   // samples taken at a different rate would distort the graph windows
        IntervalChanged?.Invoke();
    }

    private async Task TickAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            var snap = await Task.Run(_hardware.Read);   // sensors read off the UI thread

            Latest = snap;                               // everything below runs on the UI thread
            _history.Add(snap);
            if (_history.Count > HistoryLength) _history.RemoveAt(0);
            Updated?.Invoke(snap);
        }
        finally { _busy = false; }
    }

    public void Dispose()
    {
        _timer.Stop();
        _hardware.Dispose();
    }
}
