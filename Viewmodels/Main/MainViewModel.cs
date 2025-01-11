using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Views;
using HyunDaiINJ.Views.Controls;
using HyunDaiINJ.Views.Monitoring;

namespace HyunDaiINJ.ViewModels.Main
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<CustomTab> CustomTabs { get; } = new();

        private CustomTab? _selectedTab;
        public CustomTab? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    Console.WriteLine($"SelectedTab changed: {_selectedTab?.CustomHeader}");
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddTabCommand { get; }
        public ICommand CloseTabCommand { get; }

        public MainViewModel()
        {
            // 명령 초기화
            AddTabCommand = new RelayCommand<string>(AddTab);
            CloseTabCommand = new RelayCommand<CustomTab>(CloseTab);
        }

        private void AddTab(string tabHeader)
        {
            Console.WriteLine($"Adding tab: {tabHeader}");

            // 이미 존재하는 탭인지 확인
            var existingTab = CustomTabs.FirstOrDefault(tab => tab.CustomHeader == tabHeader);
            if (existingTab != null)
            {
                SelectedTab = existingTab;
                return;
            }

            // 새로운 탭 콘텐츠 생성
            object? controlContent = tabHeader switch
            {
                "품질 모니터링" => new Views.Monitoring.Pages.Monitoring.VisionMonitoring(), // Monitoring Page
                "품질 통계" => new Views.Monitoring.Pages.Monitoring.VisionStat(), // Monitoring Page
                "생산계획/지시" => new Views.Plan.Pages.WeekPlan(), // 생산계획 예시 (Page 또는 Control)
                "생산실적" => new DataGrid(), // 도넛 차트
                _ => null
            };

            if (controlContent != null)
            {
                var newTab = new CustomTab
                {
                    CustomHeader = tabHeader,
                    CustomContent = controlContent,
                    CloseCommand = CloseTabCommand
                };

                CustomTabs.Add(newTab);
                SelectedTab = newTab;
            }
        }

        private void CloseTab(CustomTab tab)
        {
            if (CustomTabs.Contains(tab))
            {
                CustomTabs.Remove(tab);

                // 닫힌 탭이 현재 선택된 탭이면 다른 탭 선택
                if (SelectedTab == tab)
                {
                    SelectedTab = CustomTabs.FirstOrDefault();
                }
            }
        }
    }
}
