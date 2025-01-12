//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HyunDaiINJ.DATA.DTO;
//using HyunDaiINJ.Models.Plan;
//using HyunDaiINJ.Services;

//namespace HyunDaiINJ.ViewModels.Plan
//{
//    public class WeekPlanViewModel : INotifyPropertyChanged
//    {
//        // 1. 주차별 계획
//        public ObservableCollection<InjectionPlanModel> WeekPlans { get; set; }

//        // 2. 일자별 계획 
//        public ObservableCollection<InjectionPlanModel> DailyPlans { get; set; }

//        private string _partIdHeader = "제품 ID";
//        public string PartIdHeader
//        {
//            get => _partIdHeader;
//            set
//            {
//                if (_partIdHeader != value)
//                {
//                    _partIdHeader = value;
//                    OnPropertyChanged(nameof(PartIdHeader));

//                    // 사용자가 제품명을 입력하면, 즉시 WeekPlans 컬렉션에 아이템 추가
//                    // (실제로는 LostFocus 시점마다 계속 추가될 수 있으니 주의)
//                    if (!string.IsNullOrWhiteSpace(_partIdHeader))
//                    {
//                        var dto = new InjectionPlanDTO
//                        {
//                            PartId = _partIdHeader,
//                            WeekQuan = 0
//                        };
//                        var model = new InjectionPlanModel(dto);

//                        // 새 아이템 추가 → DataGrid에 새 행 생성
//                        WeekPlans.Add(model);

//                        // 만약 입력 직후 TextBox를 비우고 싶다면
//                        // PartIdHeader = "";
//                    }
//                }
//            }
//        }


        

//        private int _totalWeekQuan;
//        public int TotalWeekQuan
//        {
//            get => _totalWeekQuan;
//            set
//            {
//                if (_totalWeekQuan != value)
//                {
//                    _totalWeekQuan = value;
//                    OnPropertyChanged(nameof(TotalWeekQuan));
//                }
//            }
//        }

//        private int _totalDailyQuan;
//        public int TotalDailyQuan
//        {
//            get => _totalDailyQuan;
//            set
//            {
//                if (_totalDailyQuan != value)
//                {
//                    _totalDailyQuan = value;
//                    OnPropertyChanged(nameof(TotalDailyQuan));
//                }
//            }
//        }

        

//        public WeekPlanViewModel()
//        {
           

//            // 주차 계획 초기화
//            WeekPlans = new ObservableCollection<InjectionPlanModel>();
//            for (int i = 1; i <= 52; i++)
//            {
//                var dto = new InjectionPlanDTO
//                {
//                    Week = i,
//                    WeekQuan = 0
//                };
//                var model = new InjectionPlanModel(dto);
//                WeekPlans.Add(model);
//            }

            
//            // WeekPlans CollectionChanged -> 합계 업데이트
//            WeekPlans.CollectionChanged += (s, e) => UpdateTotalWeekQuan();

//            // 각 아이템 PropertyChanged → 합계 다시 계산
//            foreach (var plan in WeekPlans)
//            {
//                plan.PropertyChanged += (s, e) =>
//                {
//                    if (e.PropertyName == nameof(InjectionPlanModel.WeekQuan))
//                    {
//                        UpdateTotalWeekQuan();
//                    }
//                };
//            }

//            UpdateTotalWeekQuan();
//        }

//        private void UpdateTotalWeekQuan()
//        {
//            TotalWeekQuan = WeekPlans.Sum(x => x.WeekQuan ?? 0);
//        }

//        // INotifyPropertyChanged 구현
//        public event PropertyChangedEventHandler PropertyChanged;
//        protected virtual void OnPropertyChanged(string propertyName)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//    }
//}
