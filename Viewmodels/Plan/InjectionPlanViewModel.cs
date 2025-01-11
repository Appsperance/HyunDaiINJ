using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Models.Plan;
using HyunDaiINJ.Services;

namespace HyunDaiINJ.ViewModels.Plan
{
    public class InjectionPlanViewModel : INotifyPropertyChanged
    {
        // 1. 주차별 계획
        public ObservableCollection<InjectionPlanModel> WeekPlans { get; set; }

        // 2. 일자별 계획 
        public ObservableCollection<InjectionPlanModel> DailyPlans { get; set; }

        private string _partIdHeader = "제품 ID";
        public string PartIdHeader
        {
            get => _partIdHeader;
            set
            {
                if (_partIdHeader != value)
                {
                    _partIdHeader = value;
                    OnPropertyChanged(nameof(PartIdHeader));
                }
            }
        }

        // "월(1/09)" 처럼 헤더에 쓸 문자열
        private string _mondayHeader;
        public string MondayHeader
        {
            get => _mondayHeader;
            set
            {
                if (_mondayHeader != value)
                {
                    _mondayHeader = value;
                    OnPropertyChanged(nameof(MondayHeader));
                }
            }
        }
        private string _tuesdayHeader;
        public string TuesdayHeader
        {
            get => _tuesdayHeader;
            set
            {
                if (_tuesdayHeader != value)
                {
                    _tuesdayHeader = value;
                    OnPropertyChanged(nameof(TuesdayHeader));
                }
            }
        }
        // "월(1/09)" 처럼 헤더에 쓸 문자열
        private string _wednesdayHeader;
        public string WednesdayHeader
        {
            get => _wednesdayHeader;
            set
            {
                if (_wednesdayHeader != value)
                {
                    _wednesdayHeader = value;
                    OnPropertyChanged(nameof(WednesdayHeader));
                }
            }
        }
        private string _thursdayHeader;
        public string ThursdayHeader
        {
            get => _thursdayHeader;
            set
            {
                if (_thursdayHeader != value)
                {
                    _thursdayHeader = value;
                    OnPropertyChanged(nameof(ThursdayHeader));
                }
            }
        }

        private string _fridayHeader;
        public string FridayHeader
        {
            get => _fridayHeader;
            set
            {
                if (_fridayHeader != value)
                {
                    _fridayHeader = value;
                    OnPropertyChanged(nameof(FridayHeader));
                }
            }
        }

        private string _saturdayHeader;
        public string SaturdayHeader
        {
            get => _saturdayHeader;
            set
            {
                if (_saturdayHeader != value)
                {
                    _saturdayHeader = value;
                    OnPropertyChanged(nameof(SaturdayHeader));
                }
            }
        }

        private string _sundayHeader;
        public string SundayHeader
        {
            get => _sundayHeader;
            set
            {
                if (_sundayHeader != value)
                {
                    _sundayHeader = value;
                    OnPropertyChanged(nameof(SundayHeader));
                }
            }
        }

        private int _totalWeekQuan;
        public int TotalWeekQuan
        {
            get => _totalWeekQuan;
            set
            {
                if (_totalWeekQuan != value)
                {
                    _totalWeekQuan = value;
                    OnPropertyChanged(nameof(TotalWeekQuan));
                }
            }
        }

        private int _totalDailyQuan;
        public int TotalDailyQuan
        {
            get => _totalDailyQuan;
            set
            {
                if (_totalDailyQuan != value)
                {
                    _totalDailyQuan = value;
                    OnPropertyChanged(nameof(TotalDailyQuan));
                }
            }
        }

        private int _currentWeekNumber;
        public int CurrentWeekNumber
        {
            get => _currentWeekNumber;
            set
            {
                if (_currentWeekNumber != value)
                {
                    _currentWeekNumber = value;
                    OnPropertyChanged(nameof(CurrentWeekNumber));
                }
            }
        }

        public InjectionPlanViewModel()
        {
            // (1) ISO 주차 계산
            int isoWeek = IsoWeekCalculator.GetIso8601WeekOfYear(DateTime.Today);
            CurrentWeekNumber = isoWeek;  // 예: 3, 4, 52 등

            // 주차 계획 초기화
            WeekPlans = new ObservableCollection<InjectionPlanModel>();
            for (int i = 1; i <= 52; i++)
            {
                var dto = new InjectionPlanDTO
                {
                    Week = i,
                    WeekQuan = 0
                };
                var model = new InjectionPlanModel(dto);
                WeekPlans.Add(model);
            }

            // 2) 요일 헤더 계산 예시
            //    "원하는 주차"의 "월요일" 계산 → MondayHeader = $"월({mondayDate:MM/dd})"
            DateTime today = DateTime.Today;
            // 예: 단순히 "오늘이 속한 주차"의 월요일 구하기 (규칙은 임의)
            // 여기서는 ( (WeekOfYear - 1)*7 ) + 1/1 로 계산 등 다양.
            // 간단 예시로 "오늘이 월요일"이라고 가정해서:
            DateTime monday = today.AddDays(-(int)today.DayOfWeek + 1);
            MondayHeader = $"월({monday:MM/dd})";
            TuesdayHeader = $"화({monday.AddDays(1):MM/dd})";
            WednesdayHeader = $"수({monday.AddDays(2):MM/dd})";
            ThursdayHeader = $"목({monday.AddDays(3):MM/dd})";
            FridayHeader = $"금({monday.AddDays(4):MM/dd})";
            SaturdayHeader = $"토({monday.AddDays(5):MM/dd})";
            SundayHeader = $"일({monday.AddDays(6):MM/dd})";
            // WeekPlans CollectionChanged -> 합계 업데이트
            WeekPlans.CollectionChanged += (s, e) => UpdateTotalWeekQuan();

            // (4) 합계 로직
            WeekPlans.CollectionChanged += (s, e) => UpdateTotalWeekQuan();
            foreach (var model in WeekPlans)
            {
                model.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(InjectionPlanModel.WeekQuan))
                        UpdateTotalWeekQuan();
                };
            }
            UpdateTotalWeekQuan();
        }   

        private void UpdateTotalWeekQuan()
        {
            TotalWeekQuan = WeekPlans.Sum(m => m.WeekQuan ?? 0);
        }

        // INotifyPropertyChanged 구현
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
