using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.ViewModels.Plan;
using HyunDaiINJ.Views;
using HyunDaiINJ.Views.Controls;
using HyunDaiINJ.Views.Monitoring;
using MyApp.ViewModels.Plan;

namespace HyunDaiINJ.ViewModels.Main
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<CustomTab> CustomTabs { get; } = new();
        //public Injecti InjectionVM { get; }
        // 예: 주간 계획 VM
        public WeekPlanViewModel WeekPlanVM { get; }
        public DailyPlanViewModel DailyPlanVM { get; }
        //// 일간 계획 VM
        //public InjectionPlanViewModel DailyPlanVM { get; }

        // 현재 선택된 탭
        private CustomTab? _selectedTab;
        public CustomTab? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged(nameof(SelectedTab));
                }
            }
        }

        // 탭 전환 시 표시할 Content(= Page, UserControl 등)
        private object _currentPage;
        public object CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                }
            }
        }

        // 명령: 탭 추가/닫기
        public ICommand AddTabCommand { get; }
        public ICommand CloseTabCommand { get; }

        // 생성자
        public MainViewModel()
        {
            // (1) 주간 계획 VM 한 번만 생성
            WeekPlanVM = new WeekPlanViewModel();
            Console.WriteLine($"WeekPlanVM : {WeekPlanVM}");
           
            DailyPlanVM = new DailyPlanViewModel();

            //InjectionVM = new InjectionPlanViewModel();
            //Console.WriteLine($"WeekPlanVM : {InjectionVM}");
            // 커맨드 생성
            AddTabCommand = new RelayCommand<string>(AddTab);
            CloseTabCommand = new RelayCommand<CustomTab>(CloseTab);
        }

        private void AddTab(string tabHeader)
        {
            Console.WriteLine($"Adding tab: {tabHeader}");

            // 이미 동일 이름 탭이 있다면 재선택만
            var existing = CustomTabs.FirstOrDefault(t => t.CustomHeader == tabHeader);
            if (existing != null)
            {
                SelectedTab = existing;
                return;
            }

            // 새로운 탭 콘텐츠 생성
            object? newContent = tabHeader switch
            {
                "생산 모니터링" => new Views.Monitoring.Pages.Monitoring.ProcessMonitoring(), // Monitoring Page
                "품질 모니터링" => new Views.Monitoring.Pages.Monitoring.VisionMonitoring(), // Monitoring Page
                "품질 통계" => new Views.Monitoring.Pages.Monitoring.VisionStat(), // Monitoring Page
                "생산계획/지시" => new Views.Plan.Pages.WeekPlan(), // 생산계획 예시 (Page 또는 Control)
                "일일계획/지시" => new Views.Plan.Pages.DailyPlan(), // 생산계획 예시 (Page 또는 Control)
                "생산실적" => new DataGrid(), // 도넛 차트
                _ => null
            };

            if (newContent != null)
            {
                var newTab = new CustomTab
                {
                    CustomHeader = tabHeader,
                    CustomContent = newContent,
                    CloseCommand = CloseTabCommand
                };
                CustomTabs.Add(newTab);
                SelectedTab = newTab;
            }
        }

        private void CloseTab(CustomTab tab)
        {
            CustomTabs.Remove(tab);
            if (SelectedTab == tab)
                SelectedTab = CustomTabs.FirstOrDefault();
        }
    }
}
