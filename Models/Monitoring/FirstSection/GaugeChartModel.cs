namespace HyunDaiINJ.Models.Monitoring.FirstSection
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
