using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionYear : UserControl
    {
        public VisionYear()
        {
            InitializeComponent();

            // 디자인 모드라면, 런타임 로직 생략
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        /// <summary>
        /// 부모(Page)에서 연간 데이터(List<VisionNgDTO>)를 전달해주면,
        /// 즉시 차트를 렌더하는 메서드
        /// </summary>
        public async void SetData(List<VisionNgDTO> yearDataList)
        {
            // (1) WebView2 초기화가 안 되어 있다면 실행
            if (WebView.CoreWebView2 == null)
            {
                await WebView.EnsureCoreWebView2Async();
            }

            // (2) Chart.js config 생성
            var chartConfig = CreateChartConfig(yearDataList);

            // (3) 직렬화
            string configJson = JsonSerializer.Serialize(chartConfig);

            // (4) HTML 생성 후 NavigateToString
            string html = GenerateHtmlContent(configJson);
            WebView.NavigateToString(html);
        }

        #region (Optional) 실시간 업데이트
        /// <summary>
        /// 만약 MQTT/Socket 등을 통해 실시간으로 연간 데이터가 갱신된다면,
        /// 이 메서드 안에서 SetData(newData) 호출 가능
        /// </summary>
        public async Task ListenToServerAsync()
        {
            // 실시간 로직 필요 시 구현
        }
        #endregion

        #region WebView2 초기화
        // 부모에서 SetData(...)를 호출하기 전에,
        // Loaded 이벤트나 EnsureCoreWebView2Async() 등으로
        // WebView가 준비됐는지 확인해도 좋습니다.
        #endregion

        /// <summary>
        /// 연간 데이터 → Chart.js config 변환
        /// 예: X축 하나(“연간”), ng_label별 bar 여러 개
        /// 혹은 YearNumber 별로 X축 다수 등 다양하게 변경 가능
        /// </summary>
        private object CreateChartConfig(List<VisionNgDTO> yearDataList)
        {
            // 예시: X축에 "연간" 1개만, ng_label별로 각각 bar
            var xLabels = new[] { "연간" };
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
            foreach (var item in yearDataList)
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
                            text = "연간 불량"
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
                                margin: 0;
                                padding: 0;
                                display: flex;
                                align-items: center;
                                justify-content: center;
                                width: 100vw;
                                height: 100vh;
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
                            {(string.IsNullOrEmpty(script)
                                ? "console.log('No initial data');"
                                : $"updateChartData({script});")}
                        </script>
                    </body>
                    </html>";
        }
    }
}
