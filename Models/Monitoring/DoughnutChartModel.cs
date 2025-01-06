using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace HyunDaiINJ.Models.Monitoring
{
    public class DoughnutChartModel
    {
        public IEnumerable<ISeries> GetSeries()
        {
            return new[]
            {
                new PieSeries<double> { Values = new[] { 300.0 }, Name = "Red" },
                new PieSeries<double> { Values = new[] { 150.0 }, Name = "Blue" },
                new PieSeries<double> { Values = new[] { 100.0 }, Name = "Yellow" },
                new PieSeries<double> { Values = new[] { 200.0 }, Name = "Green" },
                new PieSeries<double> { Values = new[] { 50.0 }, Name = "Purple" }
            };
        }
    }
}
