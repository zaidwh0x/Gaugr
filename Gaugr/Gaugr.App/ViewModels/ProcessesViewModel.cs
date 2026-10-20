using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gaugr.App.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;

namespace Gaugr.App.ViewModels;

public partial class ProcessesViewModel : ObservableObject
{
    private readonly ProcessSampler _sampler = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(2) };
    private List<ProcessInfo> _all = new();
    private bool _busy;

    [ObservableProperty] private string _search = "";
    [ObservableProperty] private string _sortBy = "Memory";   // Name / CPU / Memory
    [ObservableProperty] private string _summary = "";

    public ObservableCollection<ProcessInfo> Processes { get; } = new();

    public ProcessesViewModel()
    {
        _timer.Tick += async (_, _) => await RefreshAsync();
    }

    // Only sample while the Processes page is on screen
    public void SetActive(bool active)
    {
        if (active)
        {
            _timer.Start();
            _ = RefreshAsync();
        }
        else
        {
            _timer.Stop();
        }
    }

    partial void OnSearchChanged(string value) => Apply();
    partial void OnSortByChanged(string value) => Apply();

    [RelayCommand]
    private void SetSort(string by) => SortBy = by;

    private async Task RefreshAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            _all = await Task.Run(_sampler.Sample);   // heavy work off the UI thread
            Apply();
        }
        finally { _busy = false; }
    }

    private void Apply()
    {
        IEnumerable<ProcessInfo> items = _all;

        var text = Search.Trim();
        if (text.Length > 0)
            items = items.Where(p => p.Name.Contains(text, StringComparison.OrdinalIgnoreCase));

        items = SortBy switch
        {
            "Name" => items.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            "CPU" => items.OrderByDescending(p => p.Cpu),
            _ => items.OrderByDescending(p => p.MemoryMb)
        };

        var list = items.ToList();
        Sync(list);
        Summary = $"{list.Count} apps  ·  {list.Sum(p => p.MemoryMb) / 1024:0.0} GB of memory";
    }

    // Updates rows in place so the list doesn't jump back to the top every 2 seconds
    private void Sync(List<ProcessInfo> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i < Processes.Count) Processes[i] = items[i];
            else Processes.Add(items[i]);
        }
        while (Processes.Count > items.Count)
            Processes.RemoveAt(Processes.Count - 1);
    }
}