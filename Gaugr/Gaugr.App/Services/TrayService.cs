namespace Gaugr.App.Services;

/// <summary>The Gaugr icon in the Windows notification area, with its menu and balloon alerts.</summary>
public sealed class TrayService : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _icon;
    private readonly System.Windows.Forms.ToolStripMenuItem _overlayItem;

    public TrayService()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Open Gaugr", null, (_, _) => OpenRequested?.Invoke());

        _overlayItem = new System.Windows.Forms.ToolStripMenuItem("Compact overlay");
        _overlayItem.Click += (_, _) => OverlayToggleRequested?.Invoke();
        menu.Items.Add(_overlayItem);

        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitRequested?.Invoke());

        _icon = new System.Windows.Forms.NotifyIcon
        {
            Icon = LoadIcon(),
            Text = "Gaugr",
            ContextMenuStrip = menu,
            Visible = true
        };

        _icon.MouseClick += (_, e) =>
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) OpenRequested?.Invoke();
        };
    }

    public event Action? OpenRequested;
    public event Action? ExitRequested;
    public event Action? OverlayToggleRequested;

    public void SetOverlayChecked(bool value) => _overlayItem.Checked = value;

    public void Notify(string title, string message) =>
        _icon.ShowBalloonTip(6000, title, message, System.Windows.Forms.ToolTipIcon.Warning);

    // the exe carries the Gaugr icon, so reuse it
    private static System.Drawing.Icon LoadIcon()
    {
        try
        {
            var path = Environment.ProcessPath;
            if (path is not null && System.Drawing.Icon.ExtractAssociatedIcon(path) is { } icon) return icon;
        }
        catch { /* fall through to the generic icon */ }

        return System.Drawing.SystemIcons.Application;
    }

    public void Dispose()
    {
        _icon.Visible = false;   // otherwise Windows leaves a ghost icon until you hover over it
        _icon.Dispose();
    }
}
