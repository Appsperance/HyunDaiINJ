using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.ViewModels.Plan;
using HyunDaiINJ.Views;
using HyunDaiINJ.Views.Controls;
using HyunDaiINJ.Views.Monitoring;
using HyunDaiINJ.Views.Monitoring.Pages.Monitoring;
using HyunDaiINJ.Views.Plan.Pages;
using Prism.Events;

namespace HyunDaiINJ.ViewModels.Main
{
    public class MainViewModel : ViewModelBase
    {
        // 예: MainViewModel도 IEventAggregator를 받음
        private readonly IEventAggregator _eventAggregator;
        // 탭 관리
        public ObservableCollection<CustomTab> CustomTabs { get; } = new();
        // 주간 계획 VM
        public WeekPlanViewModel WeekPlanVM { get; }
        // 일간 계획 VM
        public DailyPlanViewModel DailyPlanVM { get; }

        // 탭 선택
        private void SelectTab(CustomTab tab)
        {
            if (CustomTabs.Contains(tab))
            {
                SelectedTab = tab;
            }
        }

        // 탭 닫기
        private void CloseTab(CustomTab tab)
        {
            if (CustomTabs.Contains(tab))
            {
                CustomTabs.Remove(tab);
                // 닫은 탭이 현재 선택 탭이면, 첫 번째 탭으로 선택 전환
                if (SelectedTab == tab)
                {
                    SelectedTab = CustomTabs.FirstOrDefault();
                }
            }
        }

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
        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyStaticChanged();  // 바인딩 갱신
                    OnPropertyStaticChanged(nameof(UserNameWithSuffix));
                }
            }
        }

        // “님” 접미사 붙여서 표시하고 싶으면:
        public string UserNameWithSuffix => string.IsNullOrEmpty(_userName)
            ? ""
            : $"{_userName}님";
        // 명령: 탭 추가/닫기
        public ICommand AddTabCommand { get; }
        public ICommand SelectTabCommand { get; }
        public ICommand CloseTabCommand { get; }
        // 생성자
        public MainViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            // (1) 주간 계획 VM 한 번만 생성
            WeekPlanVM = new WeekPlanViewModel(_eventAggregator);
            Console.WriteLine($"WeekPlanVM : {WeekPlanVM}");

            DailyPlanVM = new DailyPlanViewModel(_eventAggregator);

            //InjectionVM = new InjectionPlanViewModel();
            //Console.WriteLine($"WeekPlanVM : {InjectionVM}");
            // 커맨드 생성
            AddTabCommand = new RelayCommand<string>(AddTab);
            CloseTabCommand = new RelayCommand<CustomTab>(CloseTab);
            SelectTabCommand = new RelayCommand<CustomTab>(SelectTab);
            // 실행 예시: 시작 시 탭 하나 추가
            AddTab("품질 모니터링");
        }

        private void AddTab(string tabHeader)
        {
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
                "생산 모니터링" => new ProcessMonitoring(),
                "품질 모니터링" => new VisionMonitoring(),
                "품질 통계" => new VisionStat(),
                "생산계획/지시" => new WeekPlan(),
                "일일계획/지시" => new DailyPlan(),
                "생산실적" => new DataGrid(),
                _ => null
            };

            if (newContent is FrameworkElement fe)
            {
                // VM 주입
                if (tabHeader == "생산계획/지시")
                {
                    fe.DataContext = WeekPlanVM;
                }
                else if (tabHeader == "일일계획/지시")
                {
                    fe.DataContext = DailyPlanVM;
                }
                // else - 필요한 경우에만 설정
            }

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


       
    }
}
