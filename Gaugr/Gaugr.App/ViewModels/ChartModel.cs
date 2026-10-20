using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Gaugr.App.ViewModels;

public partial class ChartModel : ObservableObject
{
    private static readonly SKColor Muted = new(138, 146, 163);
    private static readonly SKColor GridColor = new(30, 35, 45);

    [ObservableProperty] private string _summary = "";

    public LineSeries<double>[] Series { get; }
    public Axis[] XAxes { get; }
    public Axis[] YAxes { get; }

    public ChartModel(params (ObservableCollection<double> Values, SKColor Color)[] lines)
    {
        Series = lines.Select(l => new LineSeries<double>
        {
            Values = l.Values,
            Stroke = new SolidColorPaint(l.Color) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(l.Color.WithAlpha(35)),
            GeometrySize = 0,
            GeometryFill = null,
            GeometryStroke = null,
            LineSmoothness = 0
        }).ToArray();

        XAxes = new[] { new Axis { IsVisible = false, MinLimit = 0 } };
        YAxes = new[]
        {
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 100,
                MinStep = 25,
                ForceStepToMin = true,
                TextSize = 12,
                LabelsPaint = new SolidColorPaint(Muted),
                SeparatorsPaint = new SolidColorPaint(GridColor) { StrokeThickness = 1 }
            }
        };
    }

    public void SetWindow(int samples) => XAxes[0].MaxLimit = samples - 1;
}