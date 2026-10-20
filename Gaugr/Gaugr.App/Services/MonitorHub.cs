using System.Windows.Threading;

namespace Gaugr.App.Services;

public sealed class MonitorHub : IDisposable
{
    public const int HistoryLength = 300;   // 5 minutes at 1 sample per second

    private readonly HardwareMonitorService _hardware = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly List<HardwareSnapshot> _history = new();
    private bool _busy;

    public HardwareSnapshot? Latest { get; private set; }
    public IReadOnlyList<HardwareSnapshot> History => _history;

    public event Action<HardwareSnapshot>? Updated;

    public void Start()
    {
        _timer.Tick += async (_, _) => await TickAsync();
        _timer.Start();
        _ = TickAsync();
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