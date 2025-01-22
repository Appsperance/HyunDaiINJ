using System;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using HyunDaiINJ.ViewModels.Monitoring.vision; // ViewModel namespace

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionDaily : UserControl
    {
        private VisionDailyViewModel _viewModel;

        public VisionDaily()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            // ViewModel 생성
            _viewModel = new VisionDailyViewModel();
            this.DataContext = _viewModel;

            // ChartScriptUpdated 구독
            _viewModel.ChartScriptUpdated += OnChartScriptUpdated;
        }

        // 이벤트 핸들러: 차트 스크립트 변경 시 SetChartScript 호출
        private async void OnChartScriptUpdated()
        {
            // UI 스레드에서 실행
            await Dispatcher.InvokeAsync(() =>
            {
                // ViewModel이 만든 ChartScript를 가져와 UI에 반영
                SetChartScript(_viewModel.ChartScript);
            });
        }

        /// <summary>
        /// ViewModel에서 생성한 ChartScript(JSON)을 받아서 차트 표시
        /// </summary>
        public async void SetChartScript(string chartJson)
        {
            if (WebView.CoreWebView2 == null)
            {
                await WebView.EnsureCoreWebView2Async();
            }

            string html = GenerateHtmlContent(chartJson);
            WebView.NavigateToString(html);
        }

        private string GenerateHtmlContent(string scriptJson)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
                </head>
                <body style='margin:0;padding:0;display:flex;align-items:center;justify-content:center;height:100vh;'>
                    <canvas id='dailyBarChart' style='width:90vw;height:90vh;'></canvas>
                    <script>
                        let myChart;
                        function updateChartData(config) {{
                            console.log('[VisionDaily] chart config:', config);
                
                            // 여기가 중요: config.options.scales.y.ticks.stepSize = 1;
                            // 다음처럼 JavaScript로 직접 주입하거나, 
                            // C#에서 config JSON 생성 시 옵션을 넣어주면 됩니다.
                            if (!config.options.scales.y.ticks) {{
                                config.options.scales.y.ticks = {{}};
                            }}
                            config.options.scales.y.ticks.stepSize = 1;

                            if(myChart) {{
                                myChart.data = config.data;
                                myChart.update('none');
                            }} else {{
                                const ctx = document.getElementById('dailyBarChart').getContext('2d');
                                myChart = new Chart(ctx, config);
                            }}
                        }}

                        {(string.IsNullOrEmpty(scriptJson)
                                    ? "console.log('No chart JSON');"
                                    : $"updateChartData({scriptJson});")}
                    </script>
                </body>
                </html>";
        }

    }
}
