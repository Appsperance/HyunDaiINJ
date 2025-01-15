using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using HyunDaiINJ.Models.Plan;
using HyunDaiINJ.ViewModels.Main;
using HyunDaiINJ.DATA.DAO;
using System.Threading.Tasks;
using System.Globalization;

namespace HyunDaiINJ.ViewModels.Plan
{
    public class WeekPlanViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<PartInfo> PartInfoList { get; }
            = new ObservableCollection<PartInfo>();

        public ObservableCollection<WeekRow> WeekPlanRows { get; }
            = new ObservableCollection<WeekRow>();

        private Dictionary<string, int> _sumDict = new Dictionary<string, int>();
        public Dictionary<string, int> SumDict
        {
            get => _sumDict;
            set
            {
                _sumDict = value;
                OnPropertyChanged(nameof(SumDict));
            }
        }

        public ICommand PlusLineCommand { get; }
        public ICommand MinusLineCommand { get; }
        public ICommand SaveAllInsertCommand { get; }

        private readonly InjectionPlanDAO _planDao;

        public WeekPlanViewModel()
        {
            _planDao = new InjectionPlanDAO();

            PartInfoList.CollectionChanged += OnPartInfoListChanged;

            int currentYear = DateTime.Now.Year;
            for (int w = 1; w <= 52; w++)
            {
                var row = new WeekRow(currentYear, w);
                WeekPlanRows.Add(row);
            }

            PlusLineCommand = new RelayCommand<object>(ExecutePlusLine);
            MinusLineCommand = new RelayCommand<object>(ExecuteMinusLine);

            // 저장 커맨드: 멀티-VALUES 방식
            SaveAllInsertCommand = new RelayCommand<object>(async obj =>
            {
                await SaveAllPlansAtOnceAsync();
            });

            RecalcSum();
        }

        private async Task SaveAllPlansAtOnceAsync()
        {
            try
            {
                // (A) List<(int partId, DateTime dateVal, int isoWeek, int qtyWeekly, string dayVal)>
                var dataList = new List<(string name, DateTime dateVal, int isoWeek, int qtyWeekly, string dayVal)>();

                // (B) WeekPlanRows x PartInfoList
                foreach (var row in WeekPlanRows)
                {
                    foreach (var part in PartInfoList)
                    {
                        if (row.QuanDict.TryGetValue(part.PartId, out int qty))
                        {
                            // 여기서 주차 시작일, 주차번호, qty, day 문자열 등을 만든다
                            DateTime dateVal = row.WeekStartDate; // 실제 날짜
                            int isoWeek = row.Week;               // 주차번호
                            int qtyWeekly = qty;                  // 계획 수량
                            // 예: "01-02" 식으로 표시하거나, 그냥 dayVal = dateVal.Day.ToString()
                            string dayVal = dateVal.ToString("ddd", new CultureInfo("ko-KR"));

                            dataList.Add((part.Name, dateVal, isoWeek, qtyWeekly, dayVal));
                        }
                    }
                }

                if (dataList.Count == 0)
                {
                    Console.WriteLine("[VM] 저장할 데이터가 없음");
                    return;
                }

                // (C) DAO 호출
                await _planDao.InsertAllPlansAtOnceAsync(dataList);

                Console.WriteLine($"[VM] 전체 Insert 성공! rowCount={dataList.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VM] SaveAllPlansAtOnceAsync 에러: {ex.Message}");
            }
        }

        private void OnPartInfoListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    if (newItem is PartInfo newPart)
                    {
                        foreach (var row in WeekPlanRows)
                        {
                            if (!row.QuanDict.ContainsKey(newPart.PartId))
                            {
                                row.QuanDict[newPart.PartId] = 0;
                            }
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (oldItem is PartInfo oldPart)
                    {
                        foreach (var row in WeekPlanRows)
                        {
                            if (row.QuanDict.ContainsKey(oldPart.PartId))
                            {
                                row.QuanDict.Remove(oldPart.PartId);
                            }
                        }
                    }
                }
            }

            RecalcSum();
        }

        public void RecalcSum()
        {
            var newSum = new Dictionary<string, int>();
            foreach (var part in PartInfoList)
            {
                int total = 0;
                foreach (var row in WeekPlanRows)
                {
                    if (row.QuanDict.TryGetValue(part.PartId, out int val))
                    {
                        total += val;
                    }
                }
                newSum[part.Name] = total;
            }
            SumDict = newSum;
        }

        private void ExecutePlusLine(object obj)
        {
            string partName = $"Part-{PartInfoList.Count + 1}";
            var newPartInfo = new PartInfo { Name = partName };
            PartInfoList.Add(newPartInfo);
            RecalcSum();
        }

        private void ExecuteMinusLine(object obj)
        {
            if (PartInfoList.Count > 0)
            {
                PartInfoList.RemoveAt(PartInfoList.Count - 1);
                RecalcSum();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
