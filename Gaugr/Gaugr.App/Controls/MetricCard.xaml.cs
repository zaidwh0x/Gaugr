using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Gaugr.App.Controls;

public partial class MetricCard : UserControl
{
    public MetricCard() => InitializeComponent();

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(MetricCard), new PropertyMetadata(""));
    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(MetricCard), new PropertyMetadata(""));
    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(MetricCard), new PropertyMetadata(""));
    public static readonly DependencyProperty CaptionProperty =
        DependencyProperty.Register(nameof(Caption), typeof(string), typeof(MetricCard), new PropertyMetadata(""));
    public static readonly DependencyProperty GlyphProperty =
        DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(MetricCard), new PropertyMetadata(""));
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(MetricCard), new PropertyMetadata(0.0));
    public static readonly DependencyProperty GaugeStartProperty =
        DependencyProperty.Register(nameof(GaugeStart), typeof(Color), typeof(MetricCard),
            new PropertyMetadata(Color.FromRgb(120, 215, 255)));
    public static readonly DependencyProperty GaugeEndProperty =
        DependencyProperty.Register(nameof(GaugeEnd), typeof(Color), typeof(MetricCard),
            new PropertyMetadata(Color.FromRgb(40, 110, 255)));
    public static readonly DependencyProperty IsAvailableProperty =
        DependencyProperty.Register(nameof(IsAvailable), typeof(bool), typeof(MetricCard), new PropertyMetadata(true));
    public static readonly DependencyProperty UnavailableTextProperty =
        DependencyProperty.Register(nameof(UnavailableText), typeof(string), typeof(MetricCard),
            new PropertyMetadata("Sensor unavailable"));
    public static readonly DependencyProperty ViewMoreCommandProperty =
        DependencyProperty.Register(nameof(ViewMoreCommand), typeof(ICommand), typeof(MetricCard), new PropertyMetadata(null));
    public static readonly DependencyProperty ViewMoreParameterProperty =
        DependencyProperty.Register(nameof(ViewMoreParameter), typeof(object), typeof(MetricCard), new PropertyMetadata(null));

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public string Unit { get => (string)GetValue(UnitProperty); set => SetValue(UnitProperty, value); }
    public string Caption { get => (string)GetValue(CaptionProperty); set => SetValue(CaptionProperty, value); }
    public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public Color GaugeStart { get => (Color)GetValue(GaugeStartProperty); set => SetValue(GaugeStartProperty, value); }
    public Color GaugeEnd { get => (Color)GetValue(GaugeEndProperty); set => SetValue(GaugeEndProperty, value); }
    public bool IsAvailable { get => (bool)GetValue(IsAvailableProperty); set => SetValue(IsAvailableProperty, value); }
    public string UnavailableText { get => (string)GetValue(UnavailableTextProperty); set => SetValue(UnavailableTextProperty, value); }
    public ICommand? ViewMoreCommand { get => (ICommand?)GetValue(ViewMoreCommandProperty); set => SetValue(ViewMoreCommandProperty, value); }
    public object? ViewMoreParameter { get => GetValue(ViewMoreParameterProperty); set => SetValue(ViewMoreParameterProperty, value); }
}
