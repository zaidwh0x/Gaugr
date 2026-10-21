using System.Windows;
using System.Windows.Media;

namespace Gaugr.App.Controls;

public class TickGauge : FrameworkElement
{
    private static readonly Pen UnlitPen = CreatePen(Color.FromRgb(42, 45, 54), 2.5);

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(TickGauge),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StartColorProperty =
        DependencyProperty.Register(nameof(StartColor), typeof(Color), typeof(TickGauge),
            new FrameworkPropertyMetadata(Color.FromRgb(120, 215, 255), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty EndColorProperty =
        DependencyProperty.Register(nameof(EndColor), typeof(Color), typeof(TickGauge),
            new FrameworkPropertyMetadata(Color.FromRgb(40, 110, 255), FrameworkPropertyMetadataOptions.AffectsRender));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);   // 0-100
    }

    public Color StartColor
    {
        get => (Color)GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    public Color EndColor
    {
        get => (Color)GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        const int ticks = 42;
        const double start = 165, sweep = 210;   // degrees, clockwise from 3 o'clock; opens at the bottom

        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        double rOut = Math.Min(ActualWidth, ActualHeight) / 2 - 4;
        if (rOut <= 0) return;

        int lit = (int)Math.Round(Math.Clamp(Value, 0, 100) / 100 * ticks);

        DrawGlow(dc, center, rOut);

        for (int i = 0; i < ticks; i++)
        {
            double t = i / (double)(ticks - 1);
            double angle = (start + t * sweep) * Math.PI / 180;
            var dir = new Vector(Math.Cos(angle), Math.Sin(angle));

            if (i < lit)
            {
                // lit ticks are longer and grow slightly towards the leading edge
                double rIn = rOut * (0.74 + 0.04 * (1 - t));
                dc.DrawLine(CreatePen(Lerp(StartColor, EndColor, t), 3), center + dir * rIn, center + dir * rOut);
            }
            else
            {
                dc.DrawLine(UnlitPen, center + dir * (rOut * 0.84), center + dir * rOut);
            }
        }
    }

    // soft coloured pool of light under the arc
    private void DrawGlow(DrawingContext dc, Point center, double rOut)
    {
        var tint = Lerp(StartColor, EndColor, 0.5);
        var brush = new RadialGradientBrush(
            Color.FromArgb(120, tint.R, tint.G, tint.B),
            Color.FromArgb(0, tint.R, tint.G, tint.B));
        brush.Freeze();
        dc.DrawEllipse(brush, null, new Point(center.X, center.Y + rOut * 0.72), rOut * 1.05, rOut * 0.3);
    }

    private static Pen CreatePen(Color color, double thickness)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        var pen = new Pen(brush, thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };
        pen.Freeze();
        return pen;
    }

    private static Color Lerp(Color a, Color b, double t) => Color.FromRgb(
        (byte)(a.R + (b.R - a.R) * t),
        (byte)(a.G + (b.G - a.G) * t),
        (byte)(a.B + (b.B - a.B) * t));
}
