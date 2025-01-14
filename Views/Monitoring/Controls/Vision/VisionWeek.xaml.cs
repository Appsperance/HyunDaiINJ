using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.Monitoring.vision;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class VisionWeek : UserControl
    {
        private readonly VisionWeekViewModel _viewModel;

        public VisionWeek()
        {
            InitializeComponent();
            _viewModel = new VisionWeekViewModel();
            DataContext = _viewModel; // ViewModel 설정

            // ViewModel의 ChartScriptUpdated 이벤트 구독
            _viewModel.ChartScriptUpdated += UpdateChart;

            // WebView2 초기화 및 서버 통신 시작
            Loaded += VisionWeek_Loaded;
        }

        private async void VisionWeek_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // WebView2 초기화
            await InitializeWebView2Async();

            // 데이터 로드
            await _viewModel.LoadVisionNgDataWeekAsync();

            // 초기 데이터를 이용해 HTML 로드
            if (!string.IsNullOrEmpty(_viewModel.ChartScript))
            {
                string initialHtml = GenerateHtmlContent(_viewModel.ChartScript);
                WebView.NavigateToString(initialHtml);
            }
            else
            {
                // 빈 차트 HTML 로드
                string initialHtml = GenerateHtmlContent(""); // 빈 차트
                WebView.NavigateToString(initialHtml);
            }

            // 서버 통신 시작
            _ = _viewModel.ListenToServerAsync();
        }

        private void UpdateChart()
        {

            if (WebView.CoreWebView2 == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_viewModel.ChartScript))
            {
                string script = $@"
                    if (window.updateChartData) {{
                        updateChartData({_viewModel.ChartScript});
                    }} else {{
                        console.error('Chart update function not defined.');
                    }}
                ";

                WebView.CoreWebView2.ExecuteScriptAsync(script).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine($"주간 차트 바 데이터 에러 : {task.Exception?.Message}");
                    }
                    else
                    {
                        Console.WriteLine("주간 차트 바 데이터 성공");
                    }
                });
            }
            else
            {
                Console.WriteLine("주간 차트 바 데이터 없음");
            }
        }

        private async Task InitializeWebView2Async()
        {
            try
            {
                await WebView.EnsureCoreWebView2Async();

                if (WebView.CoreWebView2 != null)
                {
                    Console.WriteLine("주간 웹뷰2 성공");
                }
                else
                {
                    Console.WriteLine("주간 웹뷰2 실패");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"주간 웹뷰2 에러: {ex.Message}");
            }
        }

        private string GenerateHtmlContent(string script)
        {
            var html = $@"
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

                        // 페이지 초기 로드 시 빈 차트 생성
                        {(!string.IsNullOrEmpty(script) ? $"updateChartData({script});" : "console.log('No initial data. Waiting for updates.');")}
                    </script>
                </body>
                </html>
            ";
            return html;
        }
    }
}
