using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.Monitoring.ThirdSection;

namespace HyunDaiINJ.Views.Monitoring.Controls.ThirdSection.Vision
{
    public partial class VisionNg : UserControl
    {
        private readonly VisionNgViewModel _viewModel;

        public VisionNg()
        {
            InitializeComponent();
            _viewModel = new VisionNgViewModel();

            // WebView2 초기화
            InitializeWebView2Async().ConfigureAwait(false);
        }

        private async Task InitializeWebView2Async()
        {
            try
            {
                await WebViewChart.EnsureCoreWebView2Async();

                if (WebViewChart.CoreWebView2 != null)
                {
                    Console.WriteLine("WebView2 initialized successfully.");
                    var chartScript = _viewModel.GenerateChartScript();
                    var htmlContent = GenerateHtmlContent(chartScript);
                    WebViewChart.NavigateToString(htmlContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing WebView2: {ex.Message}");
            }
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
                    <canvas id='chart1'></canvas>
                    <script>
                        {script}
                    </script>
                </body>
                </html>
            ";
        }
    }
}
