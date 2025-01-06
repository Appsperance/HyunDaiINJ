using System.Collections.Generic;
using System.Linq;
using HyunDaiINJ.Models.Monitoring;
using LiveCharts.Wpf;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace HyunDaiINJ.ViewModels.Monitoring
{
    public class DoughnutChartViewModel
    {
        private readonly DoughnutChartModel doughnutChartModel;

        public IEnumerable<ISeries> series { get; set; }

        public DoughnutChartViewModel()
        {
            doughnutChartModel = new DoughnutChartModel();
            series = doughnutChartModel.GetSeries();
        }

        // Chart.js용 JavaScript 데이터 생성
        public string GenerateChartScript()
        {
            var data = string.Join(",", series.OfType<PieSeries<double>>().Select(s => s.Values?.FirstOrDefault() ?? 0));
            var labels = string.Join(",", series.OfType<PieSeries<double>>().Select(s => $"'{s.Name ?? "Unnamed"}'"));

            return $@"
                const data = {{
                    labels: [{labels}],
                    datasets: [{{
                        label: 'Dataset 1',
                        data: [{data}],
                        backgroundColor: [
                            'rgba(255, 99, 132, 0.5)',
                            'rgba(54, 162, 235, 0.5)',
                            'rgba(255, 206, 86, 0.5)',
                            'rgba(75, 192, 192, 0.5)',
                            'rgba(153, 102, 255, 0.5)'
                        ],
                        hoverOffset: 4
                    }}]
                }};

                const config = {{
                        type: 'doughnut',
                        data: data,
                        options: {{
                            responsive: true,
                            plugins: {{
                                legend: {{
                                    position: 'right', // 레전드를 차트 오른쪽에 배치
                                    labels: {{
                                        font: {{
                                            size: 14 // 레전드 폰트 크기 조정
                                        }}
                                    }}
                                }}
                            }},
                            layout: {{
                                padding: {{
                                    left: 0,
                                    right: 0,
                                    top: 0,
                                    bottom: 0 // 차트를 꽉 채우도록 여백 제거
                                }}
                            }}
                        }}
                    }};

                const ctx = document.getElementById('myChart').getContext('2d');
                new Chart(ctx, config);
                ";
        }
    }
}
