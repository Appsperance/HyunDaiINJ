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
using HyunDaiINJ.ViewModels.Monitoring.vision;
using HyunDaiINJ.ViewModels.Monitoring.Vision;

namespace HyunDaiINJ.Views.Monitoring.Pages.Monitoring
{
    /// <summary>
    /// VisionStat.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VisionStat : Page
    {
        private readonly VisionNgViewModel _viewModel;
        private readonly VisionYearViewModel _yearViewModel;
        private readonly VisionDailyViewModel _dailyViewModel;
        private readonly VisionWeekViewModel _weekViewModel;
        public VisionStat()
        {
            InitializeComponent();

            // 1) 부모 ViewModel 생성 → 한꺼번에 로드
            _viewModel = new VisionNgViewModel();

            // 2) DataContext = _viewModel (DataGrid 등에서 쓰고 싶다면)
            this.DataContext = _viewModel;
            _yearViewModel = new VisionYearViewModel(); // ← 추가
            _dailyViewModel = new VisionDailyViewModel(); // ← 추가
            _weekViewModel = new VisionWeekViewModel(); // ← 추가
            // 3) Loaded 이벤트에서 자식 차트에 데이터 전달
            Loaded += VisionStat_Loaded;
        }

        private async void VisionStat_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // VisionDaily, VisionWeek, VisionYear는 x:Name으로 참조 가능(예: XAML에 x:Name="DailyChart")

            //DailyChart.SetData(_viewModel.DailyData);
            //WeekChart.SetData(_viewModel.WeekData);
            // 윈도우가 로드될 때, 서버에서 데이터 가져옴
            await _viewModel.LoadDataFromServerAsync();
            await _yearViewModel.LoadVisionNgDataYearAsync();
            await _dailyViewModel.LoadVisionNgDataDailyAsync();
            await _weekViewModel.LoadVisionNgDataWeekAsync();

            // 이제 세 개 차트가 거의 동시에 Render를 시작하게 되어
            // 뒤죽박죽 순서가 아니라, 한꺼번에 표시됨
            YearChart.SetData(_viewModel.NgDetailedData);
            YearChart.SetData(_yearViewModel.YearLabelSummaries);
            WeekChart.SetData(_weekViewModel.ChartScript);
            DailyChart.SetChartScript(_dailyViewModel.ChartScript);
        }
    }
}
