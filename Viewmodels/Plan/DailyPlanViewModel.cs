using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.DATA.DTO;   // <-- DTO
using HyunDaiINJ.Models.Plan;
using HyunDaiINJ.Services;
using System.Windows.Input;
using Prism.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HyunDaiINJ.ViewModels
{
    public class DailyPlanViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly InjectionPlanDAO _dao;
        private readonly MSDApi _msdApi;  // 주차 번호 등 API 호출 담당

        // "월"→0, "화"→1, "수"→2, "목"→3, "금"→4, "토"→5, "일"→6
        private static readonly Dictionary<string, int> DayMapping = new()
        {
            { "월", 0 },
            { "화", 1 },
            { "수", 2 },
            { "목", 3 },
            { "금", 4 },
            { "토", 5 },
            { "일", 6 }
        };

        public ICommand SendAllInsertCommand { get; }

        public DailyPlanViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _dao = new InjectionPlanDAO();

            // MSDApi 인스턴스 생성 (또는 DI)
            _msdApi = new MSDApi();

            SendAllInsertCommand = new DelegateCommand(async () => await SaveAllPlansAsync());

            // 필요 시 이벤트 구독
            _eventAggregator.GetEvent<DataInsertedEvent>().Subscribe(OnDataInserted);

            // 초기 날짜/주차 설정
            DateTime today = DateTime.Today;
            SelectedDate = today;
        }

        #region 바인딩 프로퍼티

        private ObservableCollection<DailyPlanModel> _dailyRows = new();
        public ObservableCollection<DailyPlanModel> DailyRows
        {
            get => _dailyRows;
            set => SetProperty(ref _dailyRows, value);
        }

        private int _currentWeekNumber;
        public int CurrentWeekNumber
        {
            get => _currentWeekNumber;
            set => SetProperty(ref _currentWeekNumber, value);
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    if (_selectedDate.HasValue)
                    {
                        // 날짜가 설정되면 서버에서 주차 번호 받아와 화면 갱신
                        _ = UpdateWeekDataByDateAsync(_selectedDate.Value);
                    }
                }
            }
        }

        private string _mondayHeader = "";
        public string MondayHeader
        {
            get => _mondayHeader;
            set => SetProperty(ref _mondayHeader, value);
        }
        private string _tuesdayHeader = "";
        public string TuesdayHeader
        {
            get => _tuesdayHeader;
            set => SetProperty(ref _tuesdayHeader, value);
        }
        private string _wednesdayHeader = "";
        public string WednesdayHeader
        {
            get => _wednesdayHeader;
            set => SetProperty(ref _wednesdayHeader, value);
        }
        private string _thursdayHeader = "";
        public string ThursdayHeader
        {
            get => _thursdayHeader;
            set => SetProperty(ref _thursdayHeader, value);
        }
        private string _fridayHeader = "";
        public string FridayHeader
        {
            get => _fridayHeader;
            set => SetProperty(ref _fridayHeader, value);
        }
        private string _saturdayHeader = "";
        public string SaturdayHeader
        {
            get => _saturdayHeader;
            set => SetProperty(ref _saturdayHeader, value);
        }
        private string _sundayHeader = "";
        public string SundayHeader
        {
            get => _sundayHeader;
            set => SetProperty(ref _sundayHeader, value);
        }

        #endregion

        #region 메서드

        private async Task UpdateWeekDataByDateAsync(DateTime date)
        {
            try
            {
                var info = await _msdApi.GetWeekNumberInfoAsync(date);

                if (info == null)
                {
                    Console.WriteLine("[UpdateWeekDataByDateAsync] 서버 응답이 null이므로 로컬 계산으로 대체");
                    info = new WeekNumberInfo
                    {
                        WeekNumber = IsoWeekCalculator.GetIso8601WeekOfYear(date),
                        Dates = new List<DateTime>()
                    };
                }

                if (info.WeekNumber <= 0)
                {
                    Console.WriteLine("[UpdateWeekDataByDateAsync] 서버로부터 올바른 주차 번호를 받지 못함. 로컬 계산으로 대체");
                    info.WeekNumber = IsoWeekCalculator.GetIso8601WeekOfYear(date);
                }

                CurrentWeekNumber = info.WeekNumber;

                // DB에서 기존 데이터 로드
                await LoadWeekDataAsync(info.WeekNumber);

                // 헤더 업데이트
                if (info.Dates != null && info.Dates.Count >= 7)
                {
                    DateTime monday = info.Dates[0];
                    MondayHeader = $"월({monday:MM/dd})";
                    TuesdayHeader = $"화({monday.AddDays(1):MM/dd})";
                    WednesdayHeader = $"수({monday.AddDays(2):MM/dd})";
                    ThursdayHeader = $"목({monday.AddDays(3):MM/dd})";
                    FridayHeader = $"금({monday.AddDays(4):MM/dd})";
                    SaturdayHeader = $"토({monday.AddDays(5):MM/dd})";
                    SundayHeader = $"일({monday.AddDays(6):MM/dd})";
                }
                else
                {
                    int isoYear = GetIso8601Year(date);
                    DateTime monday = IsoWeekCalculator.FirstDayOfIsoWeek(isoYear, info.WeekNumber);
                    MondayHeader = $"월({monday:MM/dd})";
                    TuesdayHeader = $"화({monday.AddDays(1):MM/dd})";
                    WednesdayHeader = $"수({monday.AddDays(2):MM/dd})";
                    ThursdayHeader = $"목({monday.AddDays(3):MM/dd})";
                    FridayHeader = $"금({monday.AddDays(4):MM/dd})";
                    SaturdayHeader = $"토({monday.AddDays(5):MM/dd})";
                    SundayHeader = $"일({monday.AddDays(6):MM/dd})";
                }

                RaisePropertyChanged(nameof(MondayHeader));
                RaisePropertyChanged(nameof(TuesdayHeader));
                RaisePropertyChanged(nameof(WednesdayHeader));
                RaisePropertyChanged(nameof(ThursdayHeader));
                RaisePropertyChanged(nameof(FridayHeader));
                RaisePropertyChanged(nameof(SaturdayHeader));
                RaisePropertyChanged(nameof(SundayHeader));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DailyPlanVM] UpdateWeekDataByDateAsync 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// DB에서 iso_week=... 레코드(DTO) 가져오기 → part_id별 GroupBy → Pivot → DailyPlanModel
        /// </summary>
        private async Task LoadWeekDataAsync(int isoWeek)
        {
            try
            {
                var dtoList = await _dao.GetPlansByWeekAsync(isoWeek);

                var grouped = dtoList.GroupBy(r => r.PartId);
                var pivotList = new List<DailyPlanModel>();

                foreach (var grp in grouped)
                {
                    var pivot = new DailyPlanModel
                    {
                        PartId = grp.Key ?? "",
                        IsoWeek = isoWeek,
                        QtyWeekly = grp.FirstOrDefault()?.QtyWeekly ?? 999
                    };

                    foreach (var row in grp)
                    {
                        switch (row.Day)
                        {
                            case "월":
                                pivot.MonQuan = row.QtyDaily ?? 0;
                                break;
                            case "화":
                                pivot.TueQuan = row.QtyDaily ?? 0;
                                break;
                            case "수":
                                pivot.WedQuan = row.QtyDaily ?? 0;
                                break;
                            case "목":
                                pivot.ThuQuan = row.QtyDaily ?? 0;
                                break;
                            case "금":
                                pivot.FriQuan = row.QtyDaily ?? 0;
                                break;
                            case "토":
                                pivot.SatQuan = row.QtyDaily ?? 0;
                                break;
                            case "일":
                                pivot.SunQuan = row.QtyDaily ?? 0;
                                break;
                        }
                    }
                    pivotList.Add(pivot);
                }

                DailyRows = new ObservableCollection<DailyPlanModel>(pivotList);
                Console.WriteLine($"[DailyPlanVM] isoWeek={isoWeek}, pivot row count={pivotList.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DailyPlanVM] LoadWeekDataAsync 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// "저장" 버튼 → DailyRows의 데이터(월~일)를 0~6으로 매핑 후 PUT 요청
        /// </summary>
        private async Task SaveAllPlansAsync()
        {
            try
            {
                // isoYear로 월요일 구함(필요 시)
                int isoYear = GetIso8601Year(DateTime.Today);
                DateTime monday = IsoWeekCalculator.FirstDayOfIsoWeek(isoYear, CurrentWeekNumber);

                // (1) DailyRows를 순회
                foreach (var row in DailyRows)
                {
                    // (2) 요일별 수량을 Dictionary("월"→수량, "화"→수량, ...)
                    var dailyQuantities = new Dictionary<string, int>
                    {
                        { "월", row.MonQuan },
                        { "화", row.TueQuan },
                        { "수", row.WedQuan },
                        { "목", row.ThuQuan },
                        { "금", row.FriQuan },
                        { "토", row.SatQuan },
                        { "일", row.SunQuan },
                    };

                    // (3) PUT 요청으로 전송
                    bool success = await _msdApi.UpdateDailyPlanAsync(
                        row.IsoWeek,
                        row.PartId,
                        dailyQuantities
                    );

                    if (!success)
                    {
                        Console.WriteLine($"[SaveAllPlansAsync] PUT 실패: PartId={row.PartId}, IsoWeek={row.IsoWeek}");
                    }
                }

                Console.WriteLine("[DailyPlanVM] SaveAllPlansAsync 완료 - PUT 요청 모두 진행");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DailyPlanVM] SaveAllPlansAsync 오류: {ex.Message}");
            }
        }

        private void OnDataInserted()
        {
            Console.WriteLine("[DailyPlanVM] 데이터 삽입됨 이벤트 수신 → 재조회");
            _ = LoadWeekDataAsync(CurrentWeekNumber);
        }

        private int GetIso8601Year(DateTime date)
        {
            int week = IsoWeekCalculator.GetIso8601WeekOfYear(date);
            if (date.Month == 1 && week >= 52) return date.Year - 1;
            if (date.Month == 12 && week == 1) return date.Year + 1;
            return date.Year;
        }
    }
}
#endregion