using System;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;
using System.Linq;
using HyunDaiINJ.DATA.DTO;
using System.Diagnostics;
using HyunDaiINJ.ViewModels.Monitoring.Vision;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionYear : UserControl
    {
        private VisionYearViewModel _viewModel;
        public VisionYear()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            // ViewModel 생성
            _viewModel = new VisionYearViewModel();
            this.DataContext = _viewModel;

            // YearLabelSummaries 속성 변경 감지
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(VisionYearViewModel.YearLabelSummaries))
                {
                    SetData(_viewModel.YearLabelSummaries);
                }
            };
        }

        public async void SetData(System.Collections.Generic.IEnumerable<VisionNgDTO> data)
        {
            if (data == null)
            {
                return;
            }

            if (WebView.CoreWebView2 == null)
            {
                await WebView.EnsureCoreWebView2Async();
            }

            var listData = data.ToList();
            var chartConfig = BuildChartConfig(listData);
            var script = System.Text.Json.JsonSerializer.Serialize(chartConfig);

            string html = GenerateHtml(script);
            WebView.NavigateToString(html);
        }

        private object BuildChartConfig(IEnumerable<VisionNgDTO> data)
        {
            var yearSet = data.Select(d => d.YearNumber).Distinct().OrderBy(x => x).ToList();
            var labelSet = data.Select(d => d.NgLabel).Distinct().ToList();

            var colorPalette = new List<string>
            {
                "rgba(75, 192, 192, 0.7)",
                "rgba(255, 99, 132, 0.7)",
                "rgba(54, 162, 235, 0.7)",
                "rgba(255, 206, 86, 0.7)",
                "rgba(153, 102, 255, 0.7)",
                "rgba(255, 159, 64, 0.7)",
                "rgba(201, 203, 207, 0.7)" // 7번째 (연한 회색)
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

                datasets.Add(new
                {
                    label,
                    data = dataList,
                    backgroundColor = colorPalette[colorIndex % colorPalette.Count],
                    borderColor = "#404040", // 선 색깔을 #404040으로 설정
                    borderWidth = 1 // 선의 두께 설정
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
                        x = new
                        {
                            grid = new
                            {
                                color = "#404040"  // x축 그리드 선 색깔을 #404040으로 설정
                            },
                            ticks = new
                            {
                                color = "#95C0FF"  // x축 tick 색상 설정
                            },
                            borderColor = "#404040"  // x축 선 색깔을 #404040으로 설정
                        },
                        y = new
                        {
                            beginAtZero = true,
                            title = new
                            {
                                display = true,
                                text = "개수"
                            },
                            grid = new
                            {
                                color = "#404040"  // y축 그리드 선 색깔을 #404040으로 설정
                            },
                            ticks = new
                            {
                                color = "#95C0FF"  // y축 tick 색상 설정
                            },
                            borderColor = "#404040"  // y축 선 색깔을 #404040으로 설정
                        }
                    }
                }
            };

            return config;
        }

        private string GenerateHtml(string script)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                  <meta charset='UTF-8'/>
                  <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
                </head>
                <body style='margin:0;padding:0;display:flex;align-items:center;justify-content:center;height:100vh;'>
                  <canvas id='stackedBarChart' style='width:90vw;height:90vh;'></canvas>
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
