using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using HyunDaiINJ.DATA.DTO;

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
        /// 외부에서 List<VisionNgDTO>를 받아 -> x축 1개(오늘), ng_label별 dataset -> 차트 표시
        /// </summary>
        public async void SetData(List<VisionNgDTO> dailyDataList)
        {
            if (dailyDataList == null || dailyDataList.Count == 0)
            {
                Console.WriteLine("[VisionDaily.SetData] no daily data");
                return;
            }

            if (WebView.CoreWebView2 == null)
            {
                await WebView.EnsureCoreWebView2Async();
            }

            var chartConfig = CreateChartConfig(dailyDataList);
            string configJson = JsonSerializer.Serialize(chartConfig);

            Console.WriteLine("[VisionDaily.SetData] configJson=" + configJson);

            string html = GenerateHtmlContent(configJson);
            WebView.NavigateToString(html);
        }

        private object CreateChartConfig(List<VisionNgDTO> dailyDataList)
        {
            // 오늘 날짜 1칸
            var xLabels = new[] { DateTime.Today.ToString("yyyy-MM-dd") };
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
                            text = "오늘 불량 (일간)"
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
            return config;
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
