using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace HyunDaiINJ.ViewModels.Monitoring
{
    public class MonitoringPageViewModel : ViewModelBase
    {
        public ISeries[] DoughnutChartSeries { get; set; } = new ISeries[]
        {
            new PieSeries<double> { Values = new double[] { 40, 30, 30 }, Name = "Category 1" },
            new PieSeries<double> { Values = new double[] { 20, 50, 30 }, Name = "Category 2" }
        };

        public ISeries[] ProgressChartSeries { get; set; } = new ISeries[]
        {
            new PieSeries<double> { Values = new double[] { 209, 891 }, Name = "Progress" }
        };

        public ISeries[] QualityChartSeries { get; set; } = new ISeries[]
        {
            new ColumnSeries<double> { Values = new double[] { 456, 98 }, Name = "Quality" }
        };

        public string Temperature => "22.8°C";
        public string Vibration => "559Hz";
    }
}