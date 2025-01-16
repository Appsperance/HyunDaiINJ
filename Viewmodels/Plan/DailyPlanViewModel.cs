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
                var dtoList = await _dao.GetPlansByWeekAsync(isoWeek); // returns List<InjectionPlanDTO>

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
        /// "저장" → DailyRows pivot 반영 → Insert (QtyWeekly 없이)
        /// </summary>
        private async Task SaveAllPlansAsync()
        {
            try
            {
                int isoYear = GetIso8601Year(DateTime.Today);
                DateTime monday = IsoWeekCalculator.FirstDayOfIsoWeek(isoYear, CurrentWeekNumber);

                // qtyWeekly 없이 qtyDaily만 담는 튜플
                var dataList = new List<(string partId, DateTime dateVal, int isoWeek, int qtyDaily, string dayVal)>();

                foreach (var row in DailyRows)
                {
                    if (row.MonQuan != 0)
                    {
                        dataList.Add((row.PartId, monday, row.IsoWeek, row.MonQuan, "월"));
                    }
                    if (row.TueQuan != 0)
                    {
                        dataList.Add((row.PartId, monday.AddDays(1), row.IsoWeek, row.TueQuan, "화"));
                    }
                    // 나머지 요일도 동일한 방식으로 추가
                    if (row.WedQuan != 0)
                    {
                        dataList.Add((row.PartId, monday.AddDays(2), row.IsoWeek, row.WedQuan, "수"));
                    }
                    if (row.ThuQuan != 0)
                    {
                        dataList.Add((row.PartId, monday.AddDays(3), row.IsoWeek, row.ThuQuan, "목"));
                    }
                    if (row.FriQuan != 0)
                    {
                        dataList.Add((row.PartId, monday.AddDays(4), row.IsoWeek, row.FriQuan, "금"));
                    }
                    if (row.SatQuan != 0)
                    {
                        dataList.Add((row.PartId, monday.AddDays(5), row.IsoWeek, row.SatQuan, "토"));
                    }
                    if (row.SunQuan != 0)
                    {
                        dataList.Add((row.PartId, monday.AddDays(6), row.IsoWeek, row.SunQuan, "일"));
                    }
                }

                if (dataList.Count == 0)
                {
                    Console.WriteLine("[DailyPlanVM] 저장할 데이터가 없음.");
                    return;
                }

                // Upsert 대신 InsertDailyPlansAtOnceAsync 호출
                await _dao.InsertDailyPlansAtOnceAsync(dataList);

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