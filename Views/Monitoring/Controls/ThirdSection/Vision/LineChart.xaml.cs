using HyunDaiINJ.ViewModels.Monitoring.ThirdSection;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HyunDaiINJ.Views.Monitoring.Controls.ThirdSection.Vision
{
    public partial class LineChart : UserControl
    {
        private readonly LineChartViewModel _viewModel;

        public LineChart()
        {
            InitializeComponent();
            _viewModel = new LineChartViewModel();

            // WebView2 초기화
            InitializeWebView2Async().ConfigureAwait(false);
        }

        private async Task InitializeWebView2Async()
        {
            try
            {
                // WebView2 초기화 대기
                await ChartWebView.EnsureCoreWebView2Async();

                // CoreWebView2 초기화 확인 후 HTML 로드
                if (ChartWebView.CoreWebView2 != null)
                {
                    Console.WriteLine("WebView2 초기화 성공. HTML 콘텐츠 로드 시작.");
                    var chartScript = _viewModel.GenerateChartScript();

                    // HTML 콘텐츠 생성 및 로드
                    var htmlContent = GenerateHtmlContent(chartScript);
                    ChartWebView.NavigateToString(htmlContent);

                    Console.WriteLine("HTML 콘텐츠가 WebView2에 성공적으로 로드되었습니다.");
                }
                else
                {
                    Console.WriteLine("CoreWebView2 초기화 실패. WebView2가 제대로 로드되지 않았습니다.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 초기화 실패: {ex.Message}");
                Console.WriteLine($"WebView2 초기화 실패: {ex.Message}");
            }
        }

        private string GenerateHtmlContent(string script)
        {
            try
            {
                Console.WriteLine("HTML 콘텐츠 생성 중...");
                var html = $@"
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
                Console.WriteLine("HTML 콘텐츠 생성 완료.");
                return html;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTML 콘텐츠 생성 실패: {ex.Message}");
                throw;
            }
        }
    }
}
