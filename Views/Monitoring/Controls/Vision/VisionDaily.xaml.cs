using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using HyunDaiINJ.DATA.DTO;
using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionDaily : UserControl
    {
        public VisionDaily()
        {
            InitializeComponent();

            // 디자인 타임 분기
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        /// <summary>
        /// 부모(VisionStat)에서 일간 데이터를 주입하면,
        /// 즉시 차트를 렌더하는 메서드
        /// </summary>
        public async void SetData(List<VisionNgDTO> dailyDataList)
        {
            // (1) WebView2 초기화가 안 되어 있으면 대기 or 별도 처리
            if (WebView.CoreWebView2 == null)
            {
                // EnsureCoreWebView2Async()
                await WebView.EnsureCoreWebView2Async();
            }

            // (2) Chart.js config 생성
            var chartConfig = CreateChartConfig(dailyDataList);

            // (3) 직렬화
            string configJson = JsonSerializer.Serialize(chartConfig);

            // (4) HTML 생성 → NavigateToString
            string html = GenerateHtmlContent(configJson);
            WebView.NavigateToString(html);
        }

        // Chart config 예시 (기존)
        private object CreateChartConfig(List<VisionNgDTO> dailyDataList)
        {
            // "오늘" x축 1개, ng_label별 dataset
            // or 여러 bar: ...
            // 아래는 기존과 동일
            // ...
            var xLabels = new[] { DateTime.Today.ToShortDateString() };
            var datasets = new List<object>();

            var colorPalette = new List<string>
            {
                "rgba(75, 192, 192, 0.7)",
                "rgba(255, 99, 132, 0.7)",
                "rgba(54, 162, 235, 0.7)",
                "rgba(255, 206, 86, 0.7)",
                "rgba(153, 102, 255, 0.7)",
                "rgba(255, 159, 64, 0.7)"
            };

            int colorIndex = 0;
            foreach (var item in dailyDataList)
            {
                datasets.Add(new
                {
                    label = item.NgLabel,
                    data = new[] { item.LabelCount },
                    backgroundColor = colorPalette[colorIndex % colorPalette.Count]
                });
                colorIndex++;
            }

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
                            text = "오늘 불량"
                        }
                    }
                }
            };
            return config;
        }

        private string GenerateHtmlContent(string script)
        {
            return $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
                        <style>
                            html, body {{
                                margin: 0; padding: 0;
                                display: flex;
                                align-items: center;
                                justify-content: center;
                                width: 100vw; height: 100vh;
                                background-color: #F4F7FB;
                            }}
                            canvas {{
                                display: block;
                                width: 90vw;
                                height: 80vh;
                            }}
                        </style>
                    </head>
                    <body>
                        <canvas id='stackedBarChart'></canvas>
                        <script>
                            let myChart;
                            function updateChartData(config) {{
                                if (myChart) {{
                                    myChart.data = config.data;
                                    myChart.update('none');
                                }} else {{
                                    const ctx = document.getElementById('stackedBarChart').getContext('2d');
                                    myChart = new Chart(ctx, config);
                                }}
                            }}
                            // 초기 로드
                            {(string.IsNullOrEmpty(script) ? "console.log('No data');"
                               : $"updateChartData({script});")}
                        </script>
                    </body>
                    </html>";
        }
    }
}
