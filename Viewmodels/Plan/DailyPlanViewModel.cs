using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.Models.Plan;
using HyunDaiINJ.Services;

namespace HyunDaiINJ.ViewModels
{
    /// <summary>
    /// DailyPlan 화면용 ViewModel (ISO 주차별 조회 + Prism EventAggregator 구독)
    /// </summary>
    public class DailyPlanViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly InjectionPlanDAO _dao;

        public DailyPlanViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _dao = new InjectionPlanDAO();

            // (1) Prism EventAggregator: "데이터 삽입됨" 이벤트 구독
            //     주차번호가 필요 없다면 parameter 없이 PubSubEvent,
            //     필요하면 PubSubEvent<int> 식으로 구현 가능
            _eventAggregator.GetEvent<DataInsertedEvent>().Subscribe(OnDataInserted);

            // (2) 오늘 날짜 기준으로 초기 ISO 주차 계산
            DateTime today = DateTime.Today;

            int isoWeek = IsoWeekCalculator.GetIso8601WeekOfYear(today);
            int isoYear = GetIso8601Year(today);

            CurrentWeekNumber = isoWeek;

            // DB 조회
            _ = LoadWeekDataAsync(CurrentWeekNumber);

            // 월요일 구하기
            DateTime monday = IsoWeekCalculator.FirstDayOfIsoWeek(isoYear, isoWeek);

            // 헤더 설정
            MondayHeader = $"월({monday:MM/dd})";
            TuesdayHeader = $"화({monday.AddDays(1):MM/dd})";
            WednesdayHeader = $"수({monday.AddDays(2):MM/dd})";
            ThursdayHeader = $"목({monday.AddDays(3):MM/dd})";
            FridayHeader = $"금({monday.AddDays(4):MM/dd})";
            SaturdayHeader = $"토({monday.AddDays(5):MM/dd})";
            SundayHeader = $"일({monday.AddDays(6):MM/dd})";

            // DatePicker 기본 값
            SelectedDate = today;
        }

        #region 바인딩용 프로퍼티

        // 1) 주차별 행들
        private ObservableCollection<DailyPlanModel> _dailyRows = new();
        public ObservableCollection<DailyPlanModel> DailyRows
        {
            get => _dailyRows;
            set => SetProperty(ref _dailyRows, value);
        }

        // 2) 현재 주차
        private int _currentWeekNumber;
        public int CurrentWeekNumber
        {
            get => _currentWeekNumber;
            set => SetProperty(ref _currentWeekNumber, value);
        }

        // 3) DatePicker에서 선택된 날짜
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    // 값 변경 시 처리
                    if (_selectedDate.HasValue)
                    {
                        int isoWeek = IsoWeekCalculator.GetIso8601WeekOfYear(_selectedDate.Value);
                        int isoYear = GetIso8601Year(_selectedDate.Value);

                        CurrentWeekNumber = isoWeek;

                        // 재조회
                        _ = LoadWeekDataAsync(isoWeek);

                        // 월요일 계산
                        DateTime monday = IsoWeekCalculator.FirstDayOfIsoWeek(isoYear, isoWeek);

                        MondayHeader = $"월({monday:MM/dd})";
                        TuesdayHeader = $"화({monday.AddDays(1):MM/dd})";
                        WednesdayHeader = $"수({monday.AddDays(2):MM/dd})";
                        ThursdayHeader = $"목({monday.AddDays(3):MM/dd})";
                        FridayHeader = $"금({monday.AddDays(4):MM/dd})";
                        SaturdayHeader = $"토({monday.AddDays(5):MM/dd})";
                        SundayHeader = $"일({monday.AddDays(6):MM/dd})";

                        // 헤더 프로퍼티가 모두 바뀌었으므로
                        RaisePropertyChanged(nameof(MondayHeader));
                        RaisePropertyChanged(nameof(TuesdayHeader));
                        RaisePropertyChanged(nameof(WednesdayHeader));
                        RaisePropertyChanged(nameof(ThursdayHeader));
                        RaisePropertyChanged(nameof(FridayHeader));
                        RaisePropertyChanged(nameof(SaturdayHeader));
                        RaisePropertyChanged(nameof(SundayHeader));
                    }
                }
            }
        }

        // 4) 월~일 헤더
        private string _mondayHeader;
        public string MondayHeader
        {
            get => _mondayHeader;
            set => SetProperty(ref _mondayHeader, value);
        }

        private string _tuesdayHeader;
        public string TuesdayHeader
        {
            get => _tuesdayHeader;
            set => SetProperty(ref _tuesdayHeader, value);
        }

        private string _wednesdayHeader;
        public string WednesdayHeader
        {
            get => _wednesdayHeader;
            set => SetProperty(ref _wednesdayHeader, value);
        }

        private string _thursdayHeader;
        public string ThursdayHeader
        {
            get => _thursdayHeader;
            set => SetProperty(ref _thursdayHeader, value);
        }

        private string _fridayHeader;
        public string FridayHeader
        {
            get => _fridayHeader;
            set => SetProperty(ref _fridayHeader, value);
        }

        private string _saturdayHeader;
        public string SaturdayHeader
        {
            get => _saturdayHeader;
            set => SetProperty(ref _saturdayHeader, value);
        }

        private string _sundayHeader;
        public string SundayHeader
        {
            get => _sundayHeader;
            set => SetProperty(ref _sundayHeader, value);
        }

        #endregion

        #region 메서드

        /// <summary>
        /// DB에서 해당 주차의 데이터를 가져와서 DailyRows에 세팅
        /// </summary>
        private async Task LoadWeekDataAsync(int isoWeek)
        {
            try
            {
                var list = await _dao.GetPlansByWeekAsync(isoWeek);
                DailyRows = new ObservableCollection<DailyPlanModel>(list);
                Console.WriteLine($"[DailyPlanVM] isoWeek={isoWeek}, {list.Count}건 조회됨");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DailyPlanVM] LoadWeekDataAsync 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// Insert 후, 다른 VM이 발행한 "DataInsertedEvent"를 수신
        /// </summary>
        private void OnDataInserted()
        {
            // 필요하다면, "현재 주차" 다시 조회
            Console.WriteLine("[DailyPlanVM] 데이터 삽입됨 이벤트 수신 → 재조회");
            _ = LoadWeekDataAsync(CurrentWeekNumber);
        }

        /// <summary>
        /// 현재 날짜의 ISO 8601 연도 계산
        /// (연말/연초가 해당 연도인지 전/다음년도인지 구분)
        /// </summary>
        private int GetIso8601Year(DateTime date)
        {
            int week = IsoWeekCalculator.GetIso8601WeekOfYear(date);
            if (date.Month == 1 && week >= 52)
            {
                return date.Year - 1;
            }
            else if (date.Month == 12 && week == 1)
            {
                return date.Year + 1;
            }
            else
            {
                return date.Year;
            }
        }

        #endregion
    }
}
