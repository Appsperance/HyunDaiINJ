using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.Models.Monitoring.Vision
{
    public class GaugeChartModel
    {
        public double Value { get; set; }
        public string Label { get; set; }

        public GaugeChartModel(double value, string label)
        {
            Value = value;
            Label = label;
        }
    }
}
