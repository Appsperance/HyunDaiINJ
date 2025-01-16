using System;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;
using System.Linq;
using HyunDaiINJ.DATA.DTO;
using System.Diagnostics;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionYear : UserControl
    {
        public VisionYear()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        public async void SetData(IEnumerable<VisionNgDTO> data)
        {
            if (data == null)
            {
                Console.WriteLine("[VisionYear.SetData] data is null");
                return;
            }

            // 로그: data가 몇 개인지, 어떤 값인지
            var listData = data.ToList();
            Console.WriteLine($"[VisionYear.SetData] data count={listData.Count}");
            foreach (var d in listData)
            {
                Console.WriteLine($"  Year={d.YearNumber}, Label={d.NgLabel}, Count={d.LabelCount}");
            }

            if (WebView.CoreWebView2 == null)
            {
                await WebView.EnsureCoreWebView2Async();
            }

            var chartConfig = BuildChartConfig(listData);
            var script = System.Text.Json.JsonSerializer.Serialize(chartConfig);

            Console.WriteLine($"[VisionYear.SetData] chart script={script}");

            string html = GenerateHtml(script);
            WebView.NavigateToString(html);
        }

        private object BuildChartConfig(IEnumerable<VisionNgDTO> data)
        {
            var yearSet = data.Select(d => d.YearNumber).Distinct().OrderBy(x => x).ToList();
            var labelSet = data.Select(d => d.NgLabel).Distinct().ToList();

            Console.WriteLine("[BuildChartConfig] yearSet=" + string.Join(",", yearSet));
            Console.WriteLine("[BuildChartConfig] labelSet=" + string.Join(",", labelSet));

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
            foreach (var label in labelSet)
            {
                var dataList = new List<int>();
                foreach (var y in yearSet)
                {
                    var item = data.FirstOrDefault(d => d.YearNumber == y && d.NgLabel == label);
                    int val = item?.LabelCount ?? 0;
                    dataList.Add(val);
                }

                // 로그: 각 라벨별 dataList
                Console.WriteLine($"[BuildChartConfig] label={label}, dataList=[{string.Join(",", dataList)}]");

                datasets.Add(new
                {
                    label,
                    data = dataList,
                    backgroundColor = colorPalette[colorIndex % colorPalette.Count]
                });
                colorIndex++;
            }

            // scales 옵션
            var config = new
            {
                type = "bar",
                data = new
                {
                    labels = yearSet,
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
                            text = "연간 불량 현황 (API)"
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
                                text = "개수"
                            }
                        }
                    }
                }
            };

            return config;
        }

        private string GenerateHtml(string script)
        {
            // 여기에 console.log() 추가로 스크립트 확인
            return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'/>
  <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
</head>
<body>
  <canvas id='stackedBarChart'></canvas>
  <script>
    let myChart;
    function updateChartData(config) {{
        console.log('Chart config from HTML side:', config);
        if (myChart) {{
            myChart.data = config.data;
            myChart.update('none');
        }} else {{
            const ctx = document.getElementById('stackedBarChart').getContext('2d');
            myChart = new Chart(ctx, config);
        }}
    }}
    {(string.IsNullOrEmpty(script)
      ? "console.log('No initial data');"
      : $"updateChartData({script});")}
  </script>
</body>
</html>";
        }
    }
}
