using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Extensions;

namespace HyunDaiINJ.ViewModels.Monitoring
{
    public class GaugeChartViewModel
    {
        public IEnumerable<ISeries> Series { get; set; } =
        GaugeGenerator.BuildSolidGauge(
            new GaugeItem(30, series =>
            {
                series.Fill = new SolidColorPaint(SKColors.YellowGreen);
                // 폰트 사이즈
                series.DataLabelsSize = 50;
                // 폰트 색깔
                series.DataLabelsPaint = new SolidColorPaint(SKColors.Red);
                // 차트 위치
                series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
                series.InnerRadius = 75;
            }),
            // 차트 두께
            new GaugeItem(GaugeItem.Background, series =>
            {
                series.InnerRadius = 75;
                series.Fill = new SolidColorPaint(new SKColor(100, 181, 246, 90));
            }));
    }
}
