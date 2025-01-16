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
using System.Globalization;
using System.ComponentModel;
using System.Linq;  // for GroupBy
using System.Windows;

namespace HyunDaiINJ.ViewModels
{
    public class DailyPlanViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly InjectionPlanDAO _dao;

        public ICommand SendAllInsertCommand { get; }

        public DailyPlanViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _dao = new InjectionPlanDAO();

            SendAllInsertCommand = new DelegateCommand(async () => await SaveAllPlansAsync());

            // EventAggregator 구독
            _eventAggregator.GetEvent<DataInsertedEvent>().Subscribe(OnDataInserted);

            DateTime today = DateTime.Today;
            int isoWeek = IsoWeekCalculator.GetIso8601WeekOfYear(today);
            int isoYear = GetIso8601Year(today);

            CurrentWeekNumber = isoWeek;

            // DB 조회
            _ = LoadWeekDataAsync(CurrentWeekNumber);

            // 월요일
            DateTime monday = IsoWeekCalculator.FirstDayOfIsoWeek(isoYear, isoWeek);

            MondayHeader = $"월({monday:MM/dd})";
            TuesdayHeader = $"화({monday.AddDays(1):MM/dd})";
            WednesdayHeader = $"수({monday.AddDays(2):MM/dd})";
            ThursdayHeader = $"목({monday.AddDays(3):MM/dd})";
            FridayHeader = $"금({monday.AddDays(4):MM/dd})";
            SaturdayHeader = $"토({monday.AddDays(5):MM/dd})";
            SundayHeader = $"일({monday.AddDays(6):MM/dd})";

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
                        int isoWeek = IsoWeekCalculator.GetIso8601WeekOfYear(_selectedDate.Value);
                        int isoYear = GetIso8601Year(_selectedDate.Value);

                        CurrentWeekNumber = isoWeek;

                        _ = LoadWeekDataAsync(isoWeek);

                        DateTime monday = IsoWeekCalculator.FirstDayOfIsoWeek(isoYear, isoWeek);

                        MondayHeader = $"월({monday:MM/dd})";
                        TuesdayHeader = $"화({monday.AddDays(1):MM/dd})";
                        WednesdayHeader = $"수({monday.AddDays(2):MM/dd})";
                        ThursdayHeader = $"목({monday.AddDays(3):MM/dd})";
                        FridayHeader = $"금({monday.AddDays(4):MM/dd})";
                        SaturdayHeader = $"토({monday.AddDays(5):MM/dd})";
                        SundayHeader = $"일({monday.AddDays(6):MM/dd})";

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
        /// DB에서 iso_week=... 레코드(DTO) 가져오기 → part_id별 GroupBy → Pivot → DailyPlanModel
        /// </summary>
        private async Task LoadWeekDataAsync(int isoWeek)
        {
            try
            {
                // 1) DAO에서 DTO 목록
                var dtoList = await _dao.GetPlansByWeekAsync(isoWeek); // returns List<InjectionPlanDTO>

                // 2) part_id별 그룹
                var grouped = dtoList.GroupBy(r => r.PartId);

                var pivotList = new List<DailyPlanModel>();

                foreach (var grp in grouped)
                {
                    // grp.Key = part_id
                    var pivot = new DailyPlanModel
                    {
                        PartId = grp.Key ?? "",
                        IsoWeek = isoWeek,
                        // DB 값 활용: grp.FirstOrDefault()?.QtyWeekly ?? 999
                        QtyWeekly = grp.FirstOrDefault()?.QtyWeekly ?? 999
                    };

                    // grp: 이 part_id의 여러 day/date 레코드
                    foreach (var row in grp)
                    {
                        // row.Day = "월","화","수","목","금","토","일"
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
        /// "저장" → DailyRows pivot 반영 → Upsert
        /// </summary>
        private async Task SaveAllPlansAsync()
        {
            try
            {
                int isoYear = GetIso8601Year(DateTime.Today);
                DateTime monday = IsoWeekCalculator.FirstDayOfIsoWeek(isoYear, CurrentWeekNumber);

                var dataList = new List<(string partId, DateTime dateVal, int isoWeek, int qtyDaily, int qtyWeekly, string dayVal)>();

                // 각 row(DailyPlanModel) → (월,화... = qtyDaily)로 펼치기
                foreach (var row in DailyRows)
                {
                    if (row.MonQuan != 0)
                    {
                        var dt = monday;
                        dataList.Add((row.PartId, dt, row.IsoWeek, row.MonQuan, row.QtyWeekly, "월"));
                    }
                    if (row.TueQuan != 0)
                    {
                        var dt = monday.AddDays(1);
                        dataList.Add((row.PartId, dt, row.IsoWeek, row.TueQuan, row.QtyWeekly, "화"));
                    }
                    if (row.WedQuan != 0)
                    {
                        var dt = monday.AddDays(2);
                        dataList.Add((row.PartId, dt, row.IsoWeek, row.TueQuan, row.QtyWeekly, "화"));
                    }
                    if (row.TueQuan != 0)
                    {
                        var dt = monday.AddDays(1);
                        dataList.Add((row.PartId, dt, row.IsoWeek, row.TueQuan, row.QtyWeekly, "화"));
                    }
                    if (row.TueQuan != 0)
                    {
                        var dt = monday.AddDays(1);
                        dataList.Add((row.PartId, dt, row.IsoWeek, row.TueQuan, row.QtyWeekly, "화"));
                    }
                    if (row.TueQuan != 0)
                    {
                        var dt = monday.AddDays(1);
                        dataList.Add((row.PartId, dt, row.IsoWeek, row.TueQuan, row.QtyWeekly, "화"));
                    }
                    if (row.TueQuan != 0)
                    {
                        var dt = monday.AddDays(1);
                        dataList.Add((row.PartId, dt, row.IsoWeek, row.TueQuan, row.QtyWeekly, "화"));
                    }
                    // 수 ~ 일 동일 ...
                }

                if (dataList.Count == 0)
                {
                    Console.WriteLine("[DailyPlanVM] 저장할 데이터가 없음.");
                    return;
                }

                await _dao.UpsertAllPlansAtOnceAsync(dataList);
                Console.WriteLine($"[DailyPlanVM] SaveAllPlansAsync 완료 - {dataList.Count}건");
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