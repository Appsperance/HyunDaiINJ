using System;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using HyunDaiINJ.ViewModels.Monitoring.vision; // ViewModel namespace

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionWeek : UserControl
    {
        private VisionWeekViewModel _viewModel;

        public VisionWeek()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            // ViewModel 생성
            _viewModel = new VisionWeekViewModel();
            this.DataContext = _viewModel;

            // (1) 차트 스크립트 업데이트 이벤트 구독
            _viewModel.ChartScriptUpdated += OnChartScriptUpdated;
        }

        private async void OnChartScriptUpdated()
        {
            // (2) ViewModel에서 만든 차트 스크립트를 가져와 SetData 호출
            //     UI 스레드에서 동작하도록 Dispatcher 사용 가능
            await Dispatcher.InvokeAsync(() =>
            {
                SetData(_viewModel.ChartScript);
            });
        }

        /// <summary>
        /// Parent(ViewModel)에서 만든 ChartScript(JSON)을 받아, 차트를 로딩
        /// </summary>
        public async void SetData(string chartJson)
        {
            if (string.IsNullOrEmpty(chartJson))
            {
                Console.WriteLine("[VisionWeek] chartJson is empty");
                return;
            }

            if (WebView.CoreWebView2 == null)
            {
                await WebView.EnsureCoreWebView2Async();
            }

            // HTML 생성
            string html = GenerateHtml(chartJson);
            WebView.NavigateToString(html);
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
                    <canvas id='myCanvas' style='width:90vw;height:90vh;'></canvas>
                    <script>
                        let myChart;
                        function updateChartData(config) {{
                            console.log('[VisionWeek] config:', config);
                            if(myChart) {{
                                myChart.data = config.data;
                                myChart.update();
                            }} else {{
                                const ctx = document.getElementById('myCanvas').getContext('2d');
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
