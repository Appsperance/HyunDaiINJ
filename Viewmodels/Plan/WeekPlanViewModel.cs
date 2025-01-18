using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Mvvm;
using Prism.Events;
using Prism.Commands;
using HyunDaiINJ.DATA.DAO;     // InjectionPlanDAO
using HyunDaiINJ.Models.Plan;
using HyunDaiINJ.Services;    // PartInfo, WeekRow, MSDApi

namespace HyunDaiINJ.ViewModels
{
    public class WeekPlanViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly InjectionPlanDAO _planDao;

        private readonly MSDApi _msdApi = new MSDApi();

        public ObservableCollection<PartInfo> PartInfoList { get; }
            = new ObservableCollection<PartInfo>();

        public ObservableCollection<WeekRow> WeekPlanRows { get; }
            = new ObservableCollection<WeekRow>();

        private Dictionary<string, int> _sumDict = new Dictionary<string, int>();
        public Dictionary<string, int> SumDict
        {
            get => _sumDict;
            set => SetProperty(ref _sumDict, value);
        }

        public ICommand PlusLineCommand { get; }
        public ICommand MinusLineCommand { get; }
        public ICommand SaveAllInsertCommand { get; }

        public WeekPlanViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _planDao = new InjectionPlanDAO();

            PartInfoList.CollectionChanged += OnPartInfoListChanged;

            // 초기: 1~52주 행
            int currentYear = DateTime.Now.Year;
            for (int w = 1; w <= 52; w++)
            {
                // 일반 행: (year, week, false)
                WeekPlanRows.Add(new WeekRow(currentYear, w));
            }

            PlusLineCommand = new DelegateCommand(OnExecutePlusLine);
            MinusLineCommand = new DelegateCommand(OnExecuteMinusLine);
            SaveAllInsertCommand = new DelegateCommand(async () => await SaveAllPlansAtOnceAsync());

            RecalcSum();
        }

        private void OnPartInfoListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    if (newItem is PartInfo p)
                    {
                        foreach (var row in WeekPlanRows)
                        {
                            if (!row.QuanDict.ContainsKey(p.PartId))
                            {
                                row.QuanDict[p.PartId] = 0;
                            }
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (oldItem is PartInfo p)
                    {
                        foreach (var row in WeekPlanRows)
                        {
                            if (row.QuanDict.ContainsKey(p.PartId))
                            {
                                row.QuanDict.Remove(p.PartId);
                            }
                        }
                    }
                }
            }

            RecalcSum();
        }

        private void OnExecutePlusLine()
        {
            int newPartId = PartInfoList.Count + 1;
            string partName = $"Part-{newPartId}";

            var newPart = new PartInfo
            {
                PartId = newPartId,
                Name = partName
            };
            PartInfoList.Add(newPart);

            RecalcSum();
        }

        private void OnExecuteMinusLine()
        {
            if (PartInfoList.Count > 0)
            {
                PartInfoList.RemoveAt(PartInfoList.Count - 1);
                RecalcSum();
            }
        }

        private async Task SaveAllInsertAsync()
        {
            // 예시용...
        }

        private async Task SaveAllPlansAtOnceAsync()
        {
            try
            {
                var dataList = new List<(string name, DateTime dateVal, int isoWeek, int qtyWeekly, string dayVal)>();

                foreach (var row in WeekPlanRows)
                {
                    foreach (var part in PartInfoList)
                    {
                        if (row.QuanDict.TryGetValue(part.PartId, out int qty) && qty != 0)
                        {
                            DateTime dateVal = row.WeekStartDate;
                            int isoWeek = row.Week;
                            string dayVal = dateVal.ToString("ddd", new CultureInfo("ko-KR"));

                            dataList.Add((part.Name, dateVal, isoWeek, qty, dayVal));
                        }
                    }
                }

                if (dataList.Count == 0)
                {
                    Console.WriteLine("[WeekPlanVM] No data to save/send.");
                    return;
                }

                // local DB insert (option)
                // await _planDao.InsertAllPlansAtOnceAsync(dataList);

                // server send
                int sendCount = 0;
                foreach (var row in WeekPlanRows)
                {
                    foreach (var part in PartInfoList)
                    {
                        if (row.QuanDict.TryGetValue(part.PartId, out int qtyWeekly) && qtyWeekly != 0)
                        {
                            bool success = await _msdApi.SendWeekPlanAsync(part.Name, row.Week, qtyWeekly);
                            if (success) sendCount++;
                        }
                    }
                }

                Console.WriteLine($"[WeekPlanVM] Sent {sendCount} items to server (non-zero only).");
                _eventAggregator.GetEvent<DataInsertedEvent>().Publish();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeekPlanVM] Error in SaveAllPlansAtOnceAsync: {ex.Message}");
            }
        }

        public void RecalcSum()
        {
            var newSum = new Dictionary<string, int>();

            // 합계 행 제외 후 계산
            foreach (var part in PartInfoList)
            {
                int total = 0;
                foreach (var row in WeekPlanRows.Where(r => !r.IsSumRow))
                {
                    if (row.QuanDict.TryGetValue(part.PartId, out int val))
                        total += val;
                }
                newSum[part.Name] = total;
            }
            SumDict = newSum;

            foreach (var row in WeekPlanRows)
                row.OnRowSumChanged();

            // 기존 합계 행 제거
            var oldSumRow = WeekPlanRows.FirstOrDefault(r => r.IsSumRow);
            if (oldSumRow != null)
            {
                WeekPlanRows.Remove(oldSumRow);
            }

            // 새 합계 행: year=0,week=0, isSumRow=true
            var sumRow = new WeekRow(0, 0, true);
            sumRow.QuanDict = new Dictionary<int, int>();

            // part별 합계 넣기
            foreach (var part in PartInfoList)
            {
                if (newSum.TryGetValue(part.Name, out int totalVal))
                    sumRow.QuanDict[part.PartId] = totalVal;
                else
                    sumRow.QuanDict[part.PartId] = 0;
            }

            WeekPlanRows.Add(sumRow);
        }
    }
}
