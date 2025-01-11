using HyunDaiINJ.ViewModels.Monitoring.ThirdSection;
using System;
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

            Console.WriteLine($"[DEBUG] sadfsdfsadfsfsadfsdf: {_viewModel.ChartScript}");
            _viewModel.ChartScriptUpdated += UpdateChart;

            Loaded += LineChart_Loaded;
        }

        private async void LineChart_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                // WebView2가 초기화되도록 보장
                await ChartWebView.EnsureCoreWebView2Async();

                if (ChartWebView.CoreWebView2 != null)
                {
                    // HTML 로드
                    string htmlContent = GenerateHtmlContent(_viewModel.ChartScript ?? "{}");
                    ChartWebView.NavigateToString(htmlContent);

                    // DOMContentLoaded 이벤트 핸들링
                    ChartWebView.CoreWebView2.DOMContentLoaded += (s, args) =>
                    {
                        Console.WriteLine("[DEBUG] DOMContentLoaded event fired.");
                        UpdateChart();
                    };
                }
                else
                {
                    Console.WriteLine("[ERROR] CoreWebView2 initialization failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] WebView2 initialization failed: {ex.Message}");
            }
        }

        private void UpdateChart()
        {
            if (ChartWebView.CoreWebView2 == null)
            {
                Console.WriteLine("[ERROR] WebView2 is not initialized. UpdateChart aborted.");
                return;
            }

            if (!string.IsNullOrEmpty(_viewModel.ChartScript))
            {
                string script = $@"
                if (window.updateChartData) {{
                    console.log('[DEBUG] Executing updateChartData...');
                    updateChartData({_viewModel.ChartScript});
                }} else {{
                    console.error('[ERROR] Chart update function not defined.');
                }}
                ";

                ChartWebView.CoreWebView2.ExecuteScriptAsync(script).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine($"[ERROR] JavaScript execution failed: {task.Exception?.Message}");
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG] JavaScript executed successfully.");
                    }
                });
            }
        }

        private async Task InitializeWebView2Async()
        {
            try
            {
                await ChartWebView.EnsureCoreWebView2Async();

                if (ChartWebView.CoreWebView2 != null)
                {
                    string htmlContent = GenerateHtmlContent(_viewModel.ChartScript ?? "{}");
                    ChartWebView.NavigateToString(htmlContent);

                    ChartWebView.CoreWebView2.DOMContentLoaded += (s, e) =>
                    {
                        UpdateChart();
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] WebView2 initialization error: {ex.Message}");
            }
        }

        private string GenerateHtmlContent(string script)
        {
            Console.WriteLine($"[DEBUG] ChartScript (HTML): {script}");
            var html = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
            <script src='https://cdn.jsdelivr.net/npm/chartjs-adapter-luxon'></script>
            <script src='https://cdn.jsdelivr.net/npm/luxon'></script>
            <style>
                html, body {{ margin: 0; padding: 0; width: 100%; height: 100%; }}
                canvas {{ width: 100%; height: 100%; }}
            </style>
        </head>
        <body>
            <canvas id='lineChart'></canvas>
            <script>
                let myChart;

                function updateChartData(config) {{
                    console.log('[DEBUG] Received ChartScript:', config);
                    if (!config || !config.data) {{
                        console.error('[ERROR] Invalid chart config:', config);
                        return;
                    }}

                    if (myChart) {{
                        myChart.data = config.data;
                        myChart.update();
                        console.log('[DEBUG] Chart updated successfully.');
                    }} else {{
                        const ctx = document.getElementById('lineChart').getContext('2d');
                        myChart = new Chart(ctx, config);
                        console.log('[DEBUG] Chart initialized successfully.');
                    }}
                }}


                document.addEventListener('DOMContentLoaded', () => {{
                    try {{
                        const initialChartConfig = {script};
                        console.log('[DEBUG] Initial Chart Config:', initialChartConfig);
                        updateChartData(initialChartConfig);
                    }} catch (error) {{
                        console.error('[ERROR] Chart initialization failed:', error);
                    }}
                }});
            </script>
        </body>
        </html>
    ";
            return html;
        }
    }
}
