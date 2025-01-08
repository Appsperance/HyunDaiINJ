using HyunDaiINJ.ViewModels.Monitoring.ThirdSection;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HyunDaiINJ.Views.Monitoring.Controls.ThirdSection
{
    public partial class LineChart : UserControl
    {
        private readonly LineChartViewModel _viewModel;

        public LineChart()
        {
            InitializeComponent();
            _viewModel = new LineChartViewModel();

            // WebView2 초기화
            InitializeWebView2();
        }

        private async void InitializeWebView2()
        {
            try
            {
                // WebView2 초기화 대기
                await ChartWebView.EnsureCoreWebView2Async();

                // CoreWebView2 초기화 확인 후 HTML 로드
                if (ChartWebView.CoreWebView2 != null)
                {
                    var htmlContent = GenerateHtmlContent(_viewModel.GenerateChartScript());
                    ChartWebView.NavigateToString(htmlContent);
                }
                else
                {
                    throw new InvalidOperationException("CoreWebView2 초기화에 실패했습니다.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 초기화 실패: {ex.Message}");
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
                    <script src='https://cdn.jsdelivr.net/npm/luxon'></script>
                    <script src='https://cdn.jsdelivr.net/npm/chartjs-adapter-luxon'></script>
                    <style>
                        html, body {{
                            margin: 0;
                            padding: 0;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            width: 100vw;
                            height: 100vh;
                            background-color: #F0F0F0;
                        }}
                        canvas {{
                            display: block;
                            max-width: 90%;
                            max-height: 90%;
                            aspect-ratio: 1;
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
