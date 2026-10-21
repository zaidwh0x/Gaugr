using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Gaugr.App.ViewModels;

namespace Gaugr.App;

public partial class MainWindow : Window
{
    private const int DwmUseImmersiveDarkMode = 20;
    private const int DwmCaptionColor = 35;   // Windows 11 only; ignored on older versions

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ShellViewModel(App.Hub, App.Settings, App.Alerts);
        SourceInitialized += (_, _) => DarkenTitleBar();
    }

    private void DarkenTitleBar()
    {
        var handle = new WindowInteropHelper(this).Handle;

        int dark = 1;
        DwmSetWindowAttribute(handle, DwmUseImmersiveDarkMode, ref dark, sizeof(int));

        int caption = 0x0D0908;   // #08090D as 0x00BBGGRR, the window background
        DwmSetWindowAttribute(handle, DwmCaptionColor, ref caption, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
