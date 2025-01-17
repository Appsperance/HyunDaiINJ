using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HyunDaiINJ.Models.Plan;
using System.Windows.Input;
using Prism.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HyunDaiINJ.DATA.DTO;  // InjectionPlanDTO
using HyunDaiINJ.Services; // MSDApi

namespace HyunDaiINJ.ViewModels
{
    public class DailyPlanViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly MSDApi _msdApi;

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

        // 저장 버튼 Command → 기존 SendAllInsertCommand 대신 SendDailyInsertCommand로 이름 통일
        public ICommand SendDailyInsertCommand { get; }

        public DailyPlanViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _msdApi = new MSDApi();

            SendDailyInsertCommand = new DelegateCommand(async () => await SaveDailyPlansAsync());

            // 디폴트 날짜: 오늘
            SelectedDate = DateTime.Today;
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

        /// <summary>
        /// 날짜 변경 시 → 서버로부터 주차 정보 + 7일치 날짜 받아옴 → LoadWeekDataAsync 호출
        /// </summary>
        private async Task UpdateWeekDataByDateAsync(DateTime date)
        {
            try
            {
                var info = await _msdApi.GetWeekNumberInfoAsync(date);
                if (info == null || info.WeekNumber <= 0)
                {
                    Console.WriteLine("[UpdateWeekDataByDateAsync] 서버 응답이 없거나 주차 번호가 0 이하 → 로컬 계산 대체");
                    info = new WeekNumberInfo
                    {
                        WeekNumber = IsoWeekCalculator.GetIso8601WeekOfYear(date),
                        Dates = new List<DateTime>()
                    };
                }

                CurrentWeekNumber = info.WeekNumber;

                // 주차별 데이터 로드
                await LoadWeekDataAsync(info.WeekNumber);

                // 날짜 헤더 (info.Dates[0]이 월요일이라고 가정)
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
                    // 로컬 계산
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
        /// 해당 주차(isoWeek)에 대한 InjectionPlanDTO 목록을 받아와서, 요일별 Pivot 후 DailyRows에 할당
        /// </summary>
        private async Task LoadWeekDataAsync(int isoWeek)
        {
            try
            {
                var dtoList = await _msdApi.GetPlanWeekDataAsync(isoWeek);
                if (dtoList == null || dtoList.Count == 0)
                {
                    Console.WriteLine($"[LoadWeekDataAsync] 주차={isoWeek}, 서버 응답 없음 or 0건");
                    // 필요하다면 빈 DailyRows 처리
                    DailyRows = new ObservableCollection<DailyPlanModel>();
                    return;
                }

                var grouped = dtoList.GroupBy(r => r.PartId);
                var pivotList = new List<DailyPlanModel>();

                foreach (var grp in grouped)
                {
                    var firstRow = grp.FirstOrDefault();
                    var pivot = new DailyPlanModel
                    {
                        PartId = grp.Key ?? "",
                        IsoWeek = isoWeek,
                        QtyWeekly = firstRow?.QtyWeekly ?? 0
                    };

                    // 요일별 수량
                    foreach (var row in grp)
                    {
                        // row.Day = "월","화","수","목","금","토","일"
                        // row.QtyDaily = 실제 일일 수량
                        switch (row.Day)
                        {
                            case "월": pivot.MonQuan = row.QtyDaily ?? 0; break;
                            case "화": pivot.TueQuan = row.QtyDaily ?? 0; break;
                            case "수": pivot.WedQuan = row.QtyDaily ?? 0; break;
                            case "목": pivot.ThuQuan = row.QtyDaily ?? 0; break;
                            case "금": pivot.FriQuan = row.QtyDaily ?? 0; break;
                            case "토": pivot.SatQuan = row.QtyDaily ?? 0; break;
                            case "일": pivot.SunQuan = row.QtyDaily ?? 0; break;
                        }
                    }

                    pivotList.Add(pivot);
                }

                DailyRows = new ObservableCollection<DailyPlanModel>(pivotList);
                Console.WriteLine($"[DailyPlanVM] LoadWeekDataAsync 완료 - week={isoWeek}, rowCount={pivotList.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DailyPlanVM] LoadWeekDataAsync 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// "저장" 버튼 → PUT 요청
        /// </summary>
        private async Task SaveDailyPlansAsync()
        {
            try
            {
                foreach (var row in DailyRows)
                {
                    // 요일별 수량을 Dictionary로 만들기
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

                    // 콘솔에 찍기: 어떤 PartId + IsoWeek, 그리고 각 요일이 몇인지
                    Console.WriteLine($"[SaveAllPlansAsync] 준비 PUT 호출. " +
                                      $"PartId={row.PartId}, IsoWeek={row.IsoWeek}");
                    foreach (var kv in dailyQuantities)
                    {
                        Console.WriteLine($"   요일={kv.Key}, 수량={kv.Value}");
                    }

                    // 실제 PUT 호출
                    bool success = await _msdApi.UpdateDailyPlanAsync(
                        row.IsoWeek,
                        row.PartId,
                        dailyQuantities
                    );

                    if (!success)
                    {
                        Console.WriteLine($"[SaveAllPlansAsync] PUT 실패 → PartId={row.PartId}");
                    }
                }

                Console.WriteLine("[DailyPlanVM] SaveAllPlansAsync 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DailyPlanVM] SaveAllPlansAsync 오류: {ex.Message}");
            }
        }

        private int GetIso8601Year(DateTime date)
        {
            int week = IsoWeekCalculator.GetIso8601WeekOfYear(date);
            if (date.Month == 1 && week >= 52) return date.Year - 1;
            if (date.Month == 12 && week == 1) return date.Year + 1;
            return date.Year;
        }

        #endregion
    }
}
