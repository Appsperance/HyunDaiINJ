using HyunDaiINJ.ViewModels.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HyunDaiINJ.Views.Monitoring.Controls
{
    /// <summary>
    /// DoughnutChart.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DoughnutChart : UserControl
    {
        private DoughnutChartViewModel doughtChartViewModel;

        public DoughnutChart()
        {
            InitializeComponent();
            doughtChartViewModel = new DoughnutChartViewModel();
            DataContext = doughtChartViewModel;
            //중복방지
            Loaded -= OnLoaded;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeWebView();
        }

        private bool WebViewInitialized = false;

        private async void InitializeWebView()
        {
            if (WebViewInitialized) return; // 중복 초기화 방지
                WebViewInitialized = true;

            try
            {
                if (WebView2Control.CoreWebView2 != null)
                {
                    Console.WriteLine("WebView2가 이미 초기화되었습니다.");
                    return;
                }

                Console.WriteLine("WebView2 초기화 시작...");
                // WebView2Control 초기화
                await WebView2Control.EnsureCoreWebView2Async();
                Console.WriteLine("WebView2 초기화 완료");

                var viewModel = DataContext as DoughnutChartViewModel;
                if (viewModel != null)
                {
                    string chartScript = viewModel.GenerateChartScript();

                    string htmlContent = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <style>
                                    body {{
                                        margin: 0;
                                        padding: 0;
                                        display: flex;
                                        align-items: center;
                                        justify-content: center;
                                        width: 100vw;
                                        height: 100vh;
                                        background-color: #1E1E1E;
                                        color: white;
                                    }}
                                    canvas {{
                                        display: block;
                                        max-width: 90%;
                                        max-height: 90%;
                                        aspect-ratio: 1;
                                    }}
                                </style>
                                <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
                            </head>
                            <body>
                                <canvas id='myChart'></canvas>
                                <script>
                                    {chartScript}
                                </script>
                            </body>
                            </html>";
                    WebView2Control.NavigateToString(htmlContent);

                    

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebView2 초기화 실패: {ex.Message}");
                MessageBox.Show($"WebView2 초기화 실패: {ex.Message}");
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            WebView2Control?.Dispose();
        }

    }
}
