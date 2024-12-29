using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

namespace HyunDaiINJ.ViewModels.Monitoring
{
    public class BarChartViewModel
    {
        public ISeries[] Series { get; set; } =
        {
        new ColumnSeries<double>
        {
            Name = "Mary",
            Values = new double[] { 2, 5, 4 }
        },
        new ColumnSeries<double>
        {
            Name = "Ana",
            Values = new double[] { 3, 1, 6 }
        }
        };

        public Axis[] XAxes { get; set; } =
        {
        new Axis
        {
            Labels = new[] { "Category 1", "Category 2", "Category 3" },
            LabelsRotation = 0,
            SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
            SeparatorsAtCenter = false,
            TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
            TicksAtCenter = true,
            ForceStepToMin = true,
            MinStep = 1
        }
        };
    }
}
