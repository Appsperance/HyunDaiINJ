using System;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

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
        /// ViewModel에서 생성한 ChartScript(JSON)을 받아서 차트 표시
        /// </summary>
        public async void SetChartScript(string chartJson)
        {
            // WebView2 초기화
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
