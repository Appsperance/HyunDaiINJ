//using System;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.ComponentModel;
//using System.Linq;
//using System.Windows.Input;
//using System.Collections.Generic;
//using HyunDaiINJ.Models.Plan;
//using HyunDaiINJ.ViewModels.Main;

//namespace HyunDaiINJ.ViewModels.Plan
//{
//    public class WeekPlanViewModel : INotifyPropertyChanged
//    {
//        // (A) 동적 제품 목록: PartInfo 객체들
//        public ObservableCollection<PartInfo> PartInfoList { get; }
//            = new ObservableCollection<PartInfo>();

//        // 52주 Row
//        public ObservableCollection<WeekRow> WeekPlanRows { get; }
//            = new ObservableCollection<WeekRow>();

//        // (B) 파트별 총합(1~52주) 저장 딕셔너리 (Key: PartName, Value: 합계)
//        private Dictionary<string, int> _sumDict = new Dictionary<string, int>();
//        public Dictionary<string, int> SumDict
//        {
//            get => _sumDict;
//            set
//            {
//                _sumDict = value;
//                OnPropertyChanged(nameof(SumDict));
//            }
//        }

//        // 커맨드들
//        public ICommand PlusLineCommand { get; }
//        public ICommand MinusLineCommand { get; }
//        public ICommand SaveAllInsertCommand { get; }

//        public WeekPlanViewModel()
//        {
//            // 1) 초기 데이터
//            PartInfoList.CollectionChanged += OnPartInfoListChanged;

//            // 예: 처음에 2개 정도 기본 제품
//            var p1 = new PartInfo { Name = "Part-A" };
//            var p2 = new PartInfo { Name = "Part-B" };
//            PartInfoList.Add(p1);
//            PartInfoList.Add(p2);
//            // 현재 로컬 PC의 연도(예: 2023, 2024, etc.)
//            int currentYear = DateTime.Now.Year;

//            // 2) WeekRow 52개 생성
//            for (int w = 1; w <= 52; w++)
//            {
//                var row = new WeekRow(currentYear,w);
//                WeekPlanRows.Add(row);
//            }

//            // 3) 커맨드
//            PlusLineCommand = new RelayCommand<object>(ExecutePlusLine);
//            MinusLineCommand = new RelayCommand<object>(ExecuteMinusLine);
//            SaveAllInsertCommand = new RelayCommand<object>(obj =>
//            {
//                // 여기에 저장 로직 배치
//            });

//            // 4) 초기 합계 계산
//            RecalcSum();
//        }

//        private void OnPartInfoListChanged(object sender, NotifyCollectionChangedEventArgs e)
//        {
//            if (e.Action == NotifyCollectionChangedAction.Add)
//            {
//                foreach (var newItem in e.NewItems)
//                {
//                    if (newItem is PartInfo newPart)
//                    {
//                        // 각 WeekRow에 QuanDict[newPart.PartId] = 0 추가
//                        foreach (var row in WeekPlanRows)
//                        {
//                            if (!row.QuanDict.ContainsKey(newPart.PartId))
//                            {
//                                row.QuanDict[newPart.PartId] = 0;
//                            }
//                        }
//                    }
//                }
//            }
//            else if (e.Action == NotifyCollectionChangedAction.Remove)
//            {
//                foreach (var oldItem in e.OldItems)
//                {
//                    if (oldItem is PartInfo oldPart)
//                    {
//                        // WeekRow QuanDict에서 제거
//                        foreach (var row in WeekPlanRows)
//                        {
//                            if (row.QuanDict.ContainsKey(oldPart.PartId))
//                            {
//                                row.QuanDict.Remove(oldPart.PartId);
//                            }
//                        }
//                    }
//                }
//            }

//            // PartInfoList 변경 시 합계 다시 계산
//            RecalcSum();
//        }

//        // (C) 합계를 재계산하는 메서드
//        public void RecalcSum()
//        {
//            var newSum = new Dictionary<string, int>();

//            // 1) PartInfoList 내 모든 파트명(PartId) 대해
//            foreach (var part in PartInfoList)
//            {
//                int total = 0;

//                // 2) 52주 Row(WeekPlanRows)를 순회하면서 해당 파트ID의 수량을 더함
//                foreach (var row in WeekPlanRows)
//                {
//                    if (row.QuanDict.TryGetValue(part.PartId, out int val))
//                    {
//                        total += val;
//                    }
//                }

//                // 3) 합계 결과를 "PartName -> total" 형태로
//                newSum[part.Name] = total;
//            }

//            // 4) 최종 딕셔너리를 SumDict에 대입
//            SumDict = newSum;
//        }

//        // 열추가
//        private void ExecutePlusLine(object obj)
//        {
//            // 임의로 새 PartInfo
//            string partName = $"Part-{PartInfoList.Count + 1}";
//            var newPartInfo = new PartInfo { Name = partName };
//            PartInfoList.Add(newPartInfo);

//            // 파트 추가 후 합계 재계산
//            RecalcSum();
//        }

//        // 열삭제
//        private void ExecuteMinusLine(object obj)
//        {
//            if (PartInfoList.Count > 0)
//            {
//                PartInfoList.RemoveAt(PartInfoList.Count - 1);
//                RecalcSum();
//            }
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//        protected void OnPropertyChanged(string name)
//            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//    }
//}
