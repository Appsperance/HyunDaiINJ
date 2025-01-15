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
using HyunDaiINJ.Services;  // PartInfo, WeekRow 등

namespace HyunDaiINJ.ViewModels
{
    public class WeekPlanViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly InjectionPlanDAO _planDao;

        // 1) PartInfoList (제품 목록)
        public ObservableCollection<PartInfo> PartInfoList { get; }
            = new ObservableCollection<PartInfo>();

        // 2) 주차별 WeekPlanRows
        public ObservableCollection<WeekRow> WeekPlanRows { get; }
            = new ObservableCollection<WeekRow>();

        // 3) 합계 (Dictionary: Key=PartName, Value=총합)
        private Dictionary<string, int> _sumDict = new Dictionary<string, int>();
        public Dictionary<string, int> SumDict
        {
            get => _sumDict;
            set => SetProperty(ref _sumDict, value);
        }

        // 4) 명령들
        public ICommand PlusLineCommand { get; }
        public ICommand MinusLineCommand { get; }
        public ICommand SaveAllInsertCommand { get; }

        // 생성자
        public WeekPlanViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _planDao = new InjectionPlanDAO();

            // (A) PartInfoList 변경 시 콜백
            PartInfoList.CollectionChanged += OnPartInfoListChanged;

            // (B) WeekPlanRows 초기화 (1~52주 생성 예시)
            int currentYear = DateTime.Now.Year;
            for (int w = 1; w <= 52; w++)
            {
                var row = new WeekRow(currentYear, w);
                WeekPlanRows.Add(row);
            }

            // (C) 명령 초기화
            PlusLineCommand = new DelegateCommand(OnExecutePlusLine);
            MinusLineCommand = new DelegateCommand(OnExecuteMinusLine);
            SaveAllInsertCommand = new DelegateCommand(async () => await SaveAllPlansAtOnceAsync());

            // (D) 합계 초기 계산
            RecalcSum();
        }

        /// <summary>
        /// PartInfoList가 변경될 때마다 각 WeekRow.QuanDict에 Key를 추가/삭제
        /// </summary>
        private void OnPartInfoListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    if (newItem is PartInfo newPart)
                    {
                        // 모든 WeekRow에 대해, 해당 PartId 키가 없으면 0으로 추가
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
                        // 모든 WeekRow에서 해당 PartId 키 제거
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

            // 변경 후 합계 재계산
            RecalcSum();
        }

        /// <summary>
        /// "열 추가" = PartInfoList에 Part 하나 더 넣기
        /// </summary>
        private void OnExecutePlusLine()
        {
            string partName = $"Part-{PartInfoList.Count + 1}";
            var newPartInfo = new PartInfo { Name = partName };
            PartInfoList.Add(newPartInfo);
            RecalcSum();
        }

        /// <summary>
        /// "열 삭제" = PartInfoList에서 마지막 항목 제거
        /// </summary>
        private void OnExecuteMinusLine()
        {
            if (PartInfoList.Count > 0)
            {
                PartInfoList.RemoveAt(PartInfoList.Count - 1);
                RecalcSum();
            }
        }

        /// <summary>
        /// DAO를 이용해 일괄 Insert
        /// </summary>
        private async Task SaveAllPlansAtOnceAsync()
        {
            try
            {
                // (1) Insert할 데이터 목록
                var dataList = new List<(string name, DateTime dateVal, int isoWeek, int qtyWeekly, string dayVal)>();

                // (2) WeekPlanRows x PartInfoList 전체 순회
                foreach (var row in WeekPlanRows)
                {
                    foreach (var part in PartInfoList)
                    {
                        if (row.QuanDict.TryGetValue(part.PartId, out int qty))
                        {
                            // 주차정보, 날짜 등 계산
                            DateTime dateVal = row.WeekStartDate;
                            int isoWeek = row.Week;   // ex) 1~52
                            int qtyWeekly = qty;
                            string dayVal = dateVal.ToString("ddd", new CultureInfo("ko-KR"));

                            dataList.Add((part.Name, dateVal, isoWeek, qtyWeekly, dayVal));
                        }
                    }
                }

                if (dataList.Count == 0)
                {
                    Console.WriteLine("[WeekPlanVM] 저장할 데이터 없음.");
                    return;
                }

                // (3) 실제 Insert
                await _planDao.InsertAllPlansAtOnceAsync(dataList);

                Console.WriteLine($"[WeekPlanVM] 전체 Insert 완료! rowCount={dataList.Count}");

                // (4) EventAggregator: "데이터 삽입됨" 이벤트를 발행
                //     주차 번호가 필요 없으면 그냥 parameter 없이 발행
                _eventAggregator.GetEvent<DataInsertedEvent>().Publish();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeekPlanVM] Insert 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// PartInfoList / WeekPlanRows의 값들을 합산
        /// </summary>
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
                // Key=part.Name, Value=총합
                newSum[part.Name] = total;
            }

            // Prism에서는 SetProperty(ref _sumDict, newSum)도 가능
            SumDict = newSum;
        }
    }
}
