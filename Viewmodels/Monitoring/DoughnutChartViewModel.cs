using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;

namespace HyunDaiINJ.ViewModels.Monitoring
{
    public class DoughnutChartViewModel
    {
        public IEnumerable<ISeries> Series { get; set; }

        public DoughnutChartViewModel()
        {
            Series = new[] { 2, 4, 1, 4, 3 }.AsPieSeries((value, series) =>
            {
                series.MaxRadialColumnWidth = 60; // 최대 반경 너비 설정
                series.Name = $"Value {value}";   // 각 시리즈의 이름 설정
            });
        }
    }

}
