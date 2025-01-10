using HyunDaiINJ.ViewModels.Monitoring.ThirdSection;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class LineChart : UserControl
    {
        private readonly LineChartViewModel _viewModel;

        public LineChart()
        {
            InitializeComponent();
            _viewModel = new LineChartViewModel();

            Console.WriteLine($"[DEBUG] Initial ChartScript: {_viewModel.ChartScript}");
            // ViewModel 이벤트 구독
            _viewModel.ChartScriptUpdated += UpdateChart;

            // WebView2 초기화 및 데이터 로드 시작
            Loaded += LineChart_Loaded;
        }

        private async void LineChart_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // WebView2 초기화
            await InitializeWebView2Async();

            // 소켓 서버 연결 및 초기 데이터 로드
            await _viewModel.ConnectToSocketServerAsync();

            // 데이터 로드 완료 후 HTML 생성
            Console.WriteLine("[DEBUG] Generating HTML with initial chart data...");
            string initialHtml = GenerateHtmlContent(_viewModel.ChartScript);
            ChartWebView.NavigateToString(initialHtml);
        }

        private void UpdateChart()
        {
            Console.WriteLine("[DEBUG] UpdateChart called.");

            if (ChartWebView.CoreWebView2 == null)
            {
                Console.WriteLine("[DEBUG] WebView2 not initialized yet. Skipping chart update.");
                return;
            }

            if (!string.IsNullOrEmpty(_viewModel.ChartScript))
            {
                Console.WriteLine("[DEBUG] Injecting updated ChartScript into WebView2.");
                string script = $@"
                    if (window.updateChartData) {{
                        updateChartData({_viewModel.ChartScript});
                    }} else {{
                        console.error('Chart update function not defined.');
                    }}
                ";

                ChartWebView.CoreWebView2.ExecuteScriptAsync(script).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine($"[DEBUG] Script execution error: {task.Exception?.Message}");
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG] Script executed successfully.");
                    }
                });
            }
            else
            {
                Console.WriteLine("[DEBUG] ChartScript is empty, nothing to update.");
            }
        }

        private async Task InitializeWebView2Async()
        {
            try
            {
                Console.WriteLine("[DEBUG] Initializing WebView2...");
                await ChartWebView.EnsureCoreWebView2Async();

                if (ChartWebView.CoreWebView2 != null)
                {
                    Console.WriteLine("[DEBUG] WebView2 initialized successfully.");

                    // WebView2 초기화 완료 후 HTML 로드
                    string htmlContent = GenerateHtmlContent(_viewModel.ChartScript ?? "{}");
                    ChartWebView.NavigateToString(htmlContent);

                    // DOMContentLoaded 이벤트 구독
                    ChartWebView.CoreWebView2.DOMContentLoaded += (s, e) =>
                    {
                        Console.WriteLine("[DEBUG] DOMContentLoaded event triggered.");
                        UpdateChart(); // DOM 로드 후 UpdateChart 실행
                    };
                }
                else
                {
                    Console.WriteLine("[DEBUG] WebView2 initialization failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] WebView2 initialization error: {ex.Message}");
            }
        }

        private string GenerateHtmlContent(string script)
        {
            Console.WriteLine($"[DEBUG] Generating HTML with script:\n{script}");
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
    <canvas id='lineChart'></canvas>
    <script>
        let myChart;

        // Chart.js 초기화 및 업데이트 함수
        function updateChartData(config) {{
    console.log('[DEBUG] updateChartData called with config:', config);

    if (!config || !config.data) {{
        console.error('[ERROR] Invalid config:', config);
        return;
    }}

    if (myChart) {{
        myChart.data = config.data;
        myChart.update('none');
    }} else {{
        const ctx = document.getElementById('lineChart').getContext('2d');
        myChart = new Chart(ctx, config);
    }}
    console.log('[DEBUG] Chart updated successfully.');
}}


        // 페이지 로드 시 초기 차트 설정
        document.addEventListener('DOMContentLoaded', () => {{
            console.log('[DEBUG] DOMContentLoaded event triggered.');

            // 서버에서 전달받은 초기 데이터 스크립트 실행
            try {{
                if (typeof initialChartConfig !== 'undefined' && initialChartConfig) {{
                    console.log('[DEBUG] Initial chart data detected:', initialChartConfig);
                    updateChartData(initialChartConfig);
                }} else {{
                    console.log('[DEBUG] No initial chart data available.');
                }}
            }} catch (error) {{
                console.error('[ERROR] Error during initial chart setup:', error);
            }}
        }});

        // 테스트용 데이터 (삭제 가능)
        const initialChartConfig = {{
            type: ""line"",
            data: {{
                labels: [""Jan"", ""Feb"", ""Mar"", ""Apr""],
                datasets: [
                    {{
                        label: ""Example Dataset"",
                        data: [10, 20, 30, 40],
                        borderColor: ""rgba(75, 192, 192, 1)"",
                        backgroundColor: ""rgba(75, 192, 192, 0.2)"",
                        fill: true,
                        tension: 0.4
                    }}
                ]
            }},
            options: {{
                responsive: true,
                scales: {{
                    x: {{ type: ""category"" }},
                    y: {{ beginAtZero: true }}
                }}
            }}
        }};

        // 테스트 호출 (삭제 가능)
        updateChartData(initialChartConfig);
    </script>
</body>
</html>

            ";
            return html;
        }


    }
}
