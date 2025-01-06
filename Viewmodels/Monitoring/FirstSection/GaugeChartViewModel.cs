using HyunDaiINJ.Models.Monitoring;
using HyunDaiINJ.Models.Monitoring.FirstSection;
using System.Collections.Generic;
using System.Linq;

namespace HyunDaiINJ.ViewModels.Monitoring.FirstSection
{
    public class GaugeChartViewModel
    {
        public List<GaugeChartModel> ChartData { get; set; }

        public GaugeChartViewModel()
        {
            // 예제 데이터
            ChartData = new List<GaugeChartModel>
            {
                new GaugeChartModel(8, "Task A"),
                new GaugeChartModel(17, "Task B"),
                new GaugeChartModel(2, "Task C")
            };
        }

        public string GenerateChartScript()
        {
            return @"
                const configs = [
                    { data: [8, 92], element: 'chart1' },
                ];

                configs.forEach(config => {
                    const ctx = document.getElementById(config.element).getContext('2d');
                    new Chart(ctx, {
                        type: 'doughnut',
                        data: {
                            labels: ['Value', 'Remaining'],
                            datasets: [{
                                data: config.data,
                                backgroundColor: ['rgba(0, 200, 169, 1)', 'rgba(200, 200, 200, 0.5)'],
                                borderWidth: 0,
                                cutout: '80%', // 내부 원 크기
                                circumference: 180, // 반원만 표시
                                rotation: 270 // 시작 위치를 아래로 설정
                            }]
                        },
                        options: {
                            responsive: true,
                            plugins: {
                                legend: { display: false },
                                tooltip: { enabled: false }
                            },
                            animation: {
                                animateScale: true,
                                animateRotate: true
                            }
                        }
                    });
                });
            ";
        }
    }
}
