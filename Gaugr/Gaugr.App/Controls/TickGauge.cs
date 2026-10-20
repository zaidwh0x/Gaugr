using System.Windows;
using System.Windows.Media;

namespace Gaugr.App.Controls;

public class TickGauge : FrameworkElement
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(TickGauge),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);   // 0-100
    }

    protected override void OnRender(DrawingContext dc)
    {
        const int ticks = 40;
        const double start = 150, sweep = 240;   // degrees, clockwise from 3 o'clock

        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        double rOut = Math.Min(ActualWidth, ActualHeight) / 2 - 4;
        double rIn = rOut * 0.82;
        int lit = (int)Math.Round(Math.Clamp(Value, 0, 100) / 100 * ticks);

        for (int i = 0; i < ticks; i++)
        {
            double t = i / (double)(ticks - 1);
            double angle = (start + t * sweep) * Math.PI / 180;
            var dir = new Vector(Math.Cos(angle), Math.Sin(angle));

            var color = i < lit ? ColorAt(t) : Color.FromRgb(60, 64, 74);
            var pen = new Pen(new SolidColorBrush(color), 3)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            dc.DrawLine(pen, center + dir * rIn, center + dir * rOut);
        }
    }

    // blue -> yellow -> red along the arc
    private static Color ColorAt(double t) => t < 0.5
        ? Lerp(Color.FromRgb(40, 140, 255), Color.FromRgb(255, 200, 40), t * 2)
        : Lerp(Color.FromRgb(255, 200, 40), Color.FromRgb(240, 50, 40), (t - 0.5) * 2);

    private static Color Lerp(Color a, Color b, double t) => Color.FromRgb(
        (byte)(a.R + (b.R - a.R) * t),
        (byte)(a.G + (b.G - a.G) * t),
        (byte)(a.B + (b.B - a.B) * t));
}