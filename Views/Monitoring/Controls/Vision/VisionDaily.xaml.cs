using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using HyunDaiINJ.DATA.DTO;
using System.Linq;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionDaily : UserControl
    {
        public VisionDaily()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        /// <summary>
        /// 외부(ViewModel)에서 만든 List<VisionNgDTO>를 받아 차트 표시
        /// or 외부에서 chartScript(JSON) 자체를 받을 수도 있음
        /// </summary>
        public async void SetData(List<VisionNgDTO> dailyDataList)
        {
            // 빈 데이터 예외
            if (dailyDataList == null || dailyDataList.Count == 0)
            {
                Console.WriteLine("[VisionDaily.SetData] no daily data");
                return;
            }

            // WebView 초기화
            if (WebView.CoreWebView2 == null)
            {
                await WebView.EnsureCoreWebView2Async();
            }

            // (A) 여기서는 dailyDataList 자체를 GroupBy etc. 해도 되지만
            //     이미 ViewModel에서 그룹화/차트Script 생성 후 넘기는 구조도 가능
            //     지금은 "직접 GroupBy -> chartConfig" 예시를 사용해도 됨

            // 아래에서는 그냥 "빈 값이 아님"만 체크했고,
            // 실제 ChartScript(JSON)은 ViewModel에서 만들어 넘기는 경우가 흔함
            // => 'dailyDataList' 에서 X축 날짜, NgLabel 등 만들 수도 있음

            // 일단 아래서는 "직접 직렬화" 샘플
            string configJson = BuildSimpleChartJson(dailyDataList);
            Console.WriteLine("[VisionDaily.SetData] configJson=" + configJson);

            string html = GenerateHtmlContent(configJson);
            WebView.NavigateToString(html);
        }

        /// <summary>
        /// 간단히 dailyDataList를 NgLabel 기준으로 LabelCount 합산 후 차트 JSON 구성
        /// (ViewModel에서 ChartScript가 준비됐으면 그걸 그냥 쓰는 편이 더 깔끔)
        /// </summary>
        private string BuildSimpleChartJson(List<VisionNgDTO> dailyDataList)
        {
            // GroupBy
            var grouped = dailyDataList
                .GroupBy(d => d.NgLabel)
                .Select(g => new
                {
                    NgLabel = g.Key,
                    LabelCount = g.Sum(x => x.LabelCount)
                })
                .ToList();

            // X축 1칸 (임의로 today)
            var xLabels = new[] { DateTime.Today.ToString("yyyy-MM-dd") };
            var colorPalette = new List<string>
            {
                "rgba(75, 192, 192, 0.7)",
                "rgba(255, 99, 132, 0.7)",
                "rgba(54, 162, 235, 0.7)",
                "rgba(255, 206, 86, 0.7)",
                "rgba(153, 102, 255, 0.7)",
                "rgba(255, 159, 64, 0.7)"
            };

            var datasets = new List<object>();
            int colorIndex = 0;
            foreach (var item in grouped)
            {
                datasets.Add(new
                {
                    label = item.NgLabel,
                    data = new[] { item.LabelCount },
                    backgroundColor = colorPalette[colorIndex % colorPalette.Count]
                });
                colorIndex++;
            }

            // Chart.js config
            var config = new
            {
                type = "bar",
                data = new
                {
                    labels = xLabels,
                    datasets = datasets
                },
                options = new
                {
                    responsive = true,
                    plugins = new
                    {
                        legend = new { position = "top" },
                        title = new
                        {
                            display = true,
                            text = "오늘 불량 (일간, NgLabel 합산)"
                        }
                    },
                    scales = new
                    {
                        y = new
                        {
                            beginAtZero = true,
                            title = new
                            {
                                display = true,
                                text = "건수"
                            }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(config);
        }

        private string GenerateHtmlContent(string script)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
</head>
<body style='margin:0;padding:0;display:flex;align-items:center;justify-content:center;height:100vh;'>
    <canvas id='stackedBarChart' style='width:90vw;height:80vh;'></canvas>
    <script>
        let myChart;
        function updateChartData(config) {{
            console.log('[VisionDaily] config:', config);
            if(myChart) {{
                myChart.data = config.data;
                myChart.update('none');
            }} else {{
                const ctx = document.getElementById('stackedBarChart').getContext('2d');
                myChart = new Chart(ctx, config);
            }}
        }}
        {(string.IsNullOrEmpty(script)
            ? "console.log('No data');"
            : $"updateChartData({script});")}
    </script>
</body>
</html>";
        }
    }
}
