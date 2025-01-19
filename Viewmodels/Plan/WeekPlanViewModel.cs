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
using HyunDaiINJ.Services;
using System.Windows;    // PartInfo, WeekRow, MSDApi

namespace HyunDaiINJ.ViewModels
{
    public class WeekPlanViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly InjectionPlanDAO _planDao;

        // (A) MSDApi 인스턴스
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

        private bool _isDataGridVisible = false;
        public bool IsDataGridVisible
        {
            get => _isDataGridVisible;
            set => SetProperty(ref _isDataGridVisible, value);
        }

        public ICommand PlusLineCommand { get; }
        public ICommand MinusLineCommand { get; }
        public ICommand SaveAllInsertCommand { get; }

        public WeekPlanViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _planDao = new InjectionPlanDAO();

            PartInfoList.CollectionChanged += OnPartInfoListChanged;

            // (B) WeekPlanRows 초기화 (1~52주 생성 예시)
            int currentYear = DateTime.Now.Year;
            for (int w = 1; w <= 52; w++)
            {
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
                    if (newItem is PartInfo newPart)
                    {
                        // PartId를 딕셔너리 키로 사용
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
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
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

        private void OnExecutePlusLine()
        {
            // 최대 6개까지만 허용
            if (PartInfoList.Count >= 6)
            {
                MessageBox.Show("열추가는 최대 개수(6개)를 초과할 수 없습니다.");
                return;
            }

            // 1) 숫자 ID (Dictionary 키)
            int newPartId = PartInfoList.Count + 1;

            // 2) 문자열 Name
            string partName = $"Part-{newPartId}";

            var newPartInfo = new PartInfo
            {
                PartId = newPartId,  // int
                Name = partName    // string
            };
            PartInfoList.Add(newPartInfo);
            // 열 추가 시 데이터그리드 표시
            IsDataGridVisible = true;
            RecalcSum();
        }

        private void OnExecuteMinusLine()
        {
            if (PartInfoList.Count > 0)
            {
                PartInfoList.RemoveAt(PartInfoList.Count - 1);
                // 데이터가 모두 제거되면 데이터그리드 숨기기
                if (PartInfoList.Count == 0)
                {
                    IsDataGridVisible = false;
                }
                RecalcSum();
            }
        }

        /// <summary>
        /// DAO를 이용해 일괄 Insert + 서버 전송 예시
        /// </summary>
        private async Task SaveAllPlansAtOnceAsync()
        {
            try
            {
                var dataList = new List<(string name, DateTime dateVal, int isoWeek, int qtyWeekly, string dayVal)>();

                foreach (var row in WeekPlanRows)
                {
                    foreach (var part in PartInfoList)
                    {
                        // Only process if QuanDict has a value AND it's non-zero
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

                // 1) Optionally insert to local DB first...
                // await _planDao.InsertAllPlansAtOnceAsync(dataList);

                // 2) Then send only non-zero items to server
                int sendCount = 0;
                foreach (var row in WeekPlanRows)
                {
                    foreach (var part in PartInfoList)
                    {
                        if (row.QuanDict.TryGetValue(part.PartId, out int qtyWeekly) && qtyWeekly != 0)
                        {
                            // Send only if qtyWeekly != 0
                            bool success = await _msdApi.SendWeekPlanAsync(
                                part.Name,
                                row.Week,
                                qtyWeekly
                            );
                            if (success)
                                sendCount++;
                        }
                    }
                }

                Console.WriteLine($"[WeekPlanVM] Sent {sendCount} items to server (non-zero only).");

                // Fire event
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
    }
}