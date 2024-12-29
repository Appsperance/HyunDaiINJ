using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using HyunDaiINJ.DTO;
using HyunDaiINJ.Views;

namespace HyunDaiINJ.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public ObservableCollection<CustomTab> CustomTabs { get; set; }
        private CustomTab _selectedTab;
        public CustomTab SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    Console.WriteLine($"SelectedTab changed: {_selectedTab?.CustomHeader}");
                    OnPropertyChanged(nameof(SelectedTab));
                }
            }
        }

        public ICommand AddTabCommand { get; set; }
        public ICommand CloseTabCommand { get; set; }

        public MainViewModel()
        {
            // 초기화
            CustomTabs = new ObservableCollection<CustomTab>();

            // 명령 초기화
            AddTabCommand = new RelayCommand<string>(AddTab);
            CloseTabCommand = new RelayCommand<CustomTab>(CloseTab);

            // 기본값
            SelectedTab = null; // 초기에는 선택된 탭이 없음
        }

        private void AddTab(string tabHeader)
        {
            CustomTab newTab;
            // 이미 존재하는 탭인지 확인
            Console.WriteLine($"Adding tab: {tabHeader}");
            if (!CustomTabs.Any(tab => tab.CustomHeader == tabHeader))
            {
                Page pageContent = null;

                // 조건문 기반으로 페이지 생성
                if (tabHeader == "실시간 모니터링")
                {
                    pageContent = new Views.Pages.DataGrid(); // View 폴더에 있는 PageTabForm.xaml
                }
                else if (tabHeader == "생산계획/지시")
                {
                    pageContent = new Views.Pages.DataGrid(); // 다른 페이지로 변경 가능
                }
                else if (tabHeader == "생산실적")
                {
                    pageContent = new Views.Pages.DataGrid(); // 또 다른 페이지
                }

                newTab = new CustomTab
                {
                    CustomHeader = tabHeader,
                    CustomContent = pageContent, // 이 부분에 page 생성후 할당?
                    CloseCommand = CloseTabCommand
                };

                CustomTabs.Add(newTab);
            }
            else
            {
                // 이미 존재하는 탭을 선택
                newTab = CustomTabs.First(tab => tab.CustomHeader == tabHeader);
            }
            SelectedTab = newTab; // 새로 추가한 탭 선택
            OnPropertyChanged(nameof(SelectedTab));
        }

        private void CloseTab(CustomTab tab)
        {
            if (CustomTabs.Contains(tab))
            {
                CustomTabs.Remove(tab);

                // 현재 닫은 탭이 선택된 상태였으면 다른 탭으로 선택
                if (SelectedTab == tab)
                {
                    SelectedTab = CustomTabs.FirstOrDefault();
                }
            }
        }
    }
}
