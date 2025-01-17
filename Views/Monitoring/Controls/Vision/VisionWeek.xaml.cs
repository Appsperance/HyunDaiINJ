using System;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionWeek : UserControl
    {
        public VisionWeek()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
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
    <canvas id='myCanvas' style='width:90vw;height:80vh;'></canvas>
    <script>
        let myChart;
        function updateChartData(config) {{
            console.log('[VisionWeek] config:', config);
            if(myChart) {{
                myChart.data = config.data;
                myChart.update('`ne');
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
