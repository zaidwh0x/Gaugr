using System.Windows;
using System.Windows.Media;

namespace Gaugr.App.Controls;

/// <summary>Tall rounded bar with a tick scale beside it, used for the health score.</summary>
public class VerticalMeter : FrameworkElement
{
    private static readonly Color Top = Color.FromRgb(45, 230, 205);
    private static readonly Color Bottom = Color.FromRgb(255, 100, 30);

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(VerticalMeter),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);   // 0-100
    }

    protected override void OnRender(DrawingContext dc)
    {
        const double barWidth = 28;
        double height = ActualHeight;
        if (height <= 0) return;

        double fraction = Math.Clamp(Value, 0, 100) / 100;
        double fillHeight = height * fraction;
        double top = height - fillHeight;

        var track = new SolidColorBrush(Color.FromRgb(26, 28, 35));
        dc.DrawRoundedRectangle(track, null, new Rect(0, 0, barWidth, height), barWidth / 2, barWidth / 2);

        if (fillHeight > 1)
        {
            var fill = new LinearGradientBrush(Top, Bottom, 90);
            double radius = Math.Min(barWidth / 2, fillHeight / 2);
            dc.DrawRoundedRectangle(fill, null, new Rect(0, top, barWidth, fillHeight), radius, radius);
        }

        const int steps = 24;
        var unlit = new Pen(new SolidColorBrush(Color.FromRgb(42, 45, 54)), 1.5);
        for (int i = 0; i <= steps; i++)
        {
            double y = Math.Clamp(height * i / steps, 1, height - 1);
            bool major = i % 4 == 0;
            double length = major ? 14 : 8;
            double x = barWidth + 16;

            var pen = y >= top
                ? new Pen(new SolidColorBrush(Lerp(Top, Bottom, y / height)), 1.5)
                : unlit;
            dc.DrawLine(pen, new Point(x, y), new Point(x + length, y));
        }
    }

    private static Color Lerp(Color a, Color b, double t) => Color.FromRgb(
        (byte)(a.R + (b.R - a.R) * t),
        (byte)(a.G + (b.G - a.G) * t),
        (byte)(a.B + (b.B - a.B) * t));
}
