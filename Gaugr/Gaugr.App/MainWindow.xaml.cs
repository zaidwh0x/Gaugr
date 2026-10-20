using System.Windows;
using Gaugr.App.ViewModels;

namespace Gaugr.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}