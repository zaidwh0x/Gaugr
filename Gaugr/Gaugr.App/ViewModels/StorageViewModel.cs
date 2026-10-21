using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Gaugr.App.ViewModels;

public sealed class DriveItem
{
    private static readonly Brush Healthy = Frozen(79, 195, 255);
    private static readonly Brush Busy = Frozen(255, 210, 60);
    private static readonly Brush Critical = Frozen(255, 90, 31);

    public DriveItem(string name, string label, string format, string kind, double totalBytes, double freeBytes)
    {
        UsedPercent = totalBytes > 0 ? (totalBytes - freeBytes) / totalBytes * 100 : 0;

        Title = string.IsNullOrWhiteSpace(label) ? name : $"{name}  {label}";
        Detail = $"{FormatSize(freeBytes)} free of {FormatSize(totalBytes)}  ·  {format}  ·  {kind}";
        UsageBrush = UsedPercent >= 90 ? Critical : UsedPercent >= 75 ? Busy : Healthy;
        StatusText = UsedPercent >= 90 ? "Almost full. Free up some space to keep Windows running smoothly." : "";
    }

    public string Title { get; }
    public string Detail { get; }
    public string StatusText { get; }
    public double UsedPercent { get; }
    public Brush UsageBrush { get; }

    private static string FormatSize(double bytes)
    {
        double gb = bytes / 1_073_741_824.0;
        return gb >= 1024 ? $"{gb / 1024:0.##} TB" : $"{gb:0} GB";
    }

    private static SolidColorBrush Frozen(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}

public partial class StorageViewModel : ObservableObject
{
    public ObservableCollection<DriveItem> Drives { get; } = new();

    public StorageViewModel() => Refresh();

    public void Refresh()
    {
        Drives.Clear();

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType is not (DriveType.Fixed or DriveType.Removable)) continue;

            try
            {
                if (!drive.IsReady) continue;

                Drives.Add(new DriveItem(
                    drive.Name.TrimEnd('\\'),
                    drive.VolumeLabel,
                    drive.DriveFormat,
                    drive.DriveType == DriveType.Removable ? "Removable" : "Local disk",
                    drive.TotalSize,
                    drive.AvailableFreeSpace));
            }
            catch (IOException) { /* drive disappeared or is not accessible */ }
            catch (UnauthorizedAccessException) { }
        }
    }
}
