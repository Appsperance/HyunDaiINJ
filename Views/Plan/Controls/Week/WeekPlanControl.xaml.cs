using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HyunDaiINJ.ViewModels.Plan;
using HyunDaiINJ.Models.Plan;

namespace HyunDaiINJ.Views.Plan.Controls.Week
{
    public partial class WeekPlanControl : UserControl
    {
        private bool _isWeekColumnAdded = false;

        public WeekPlanControl()
        {
            InitializeComponent();
            Loaded += WeekPlanControl_Loaded;
        }

        private void WeekPlanControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as WeekPlanViewModel;
            if (vm == null) return;

            // PartInfoList의 추가/삭제 시 열을 동적으로 추가/삭제
            vm.PartInfoList.CollectionChanged += PartInfoList_CollectionChanged;

            // 처음에 '주차' 열 + 기존 PartInfoList 열들 생성
            EnsureWeekColumn();

            foreach (var part in vm.PartInfoList)
            {
                AddOneColumn(part);
            }
        }

        private void PartInfoList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (WeekDataGrid == null) return;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    if (newItem is PartInfo newPart)
                    {
                        AddOneColumn(newPart);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (oldItem is PartInfo oldPart)
                    {
                        RemoveOneColumn(oldPart);
                    }
                }
            }
        }

        private void EnsureWeekColumn()
        {
            if (_isWeekColumnAdded) return;

            var weekColumn = new DataGridTextColumn
            {
                Header = "주차",
                Binding = new Binding("Week"),
                Width = 60,
                IsReadOnly = true
            };
            WeekDataGrid.Columns.Add(weekColumn);

            _isWeekColumnAdded = true;
        }

        private void AddOneColumn(PartInfo part)
        {
            // 중복 체크: PartId로 식별
            var existing = WeekDataGrid.Columns
                .OfType<DataGridTextColumn>()
                .FirstOrDefault(c => c.SortMemberPath == $"QuanDict[{part.PartId}]");

            if (existing != null)
                return;

            // 새 컬럼
            var newCol = new DataGridTextColumn
            {
                // 헤더는 PartInfo.Name
                Header = part.Name,

                // Dictionary Key = PartId
                Binding = new Binding($"QuanDict[{part.PartId}]")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                },
                Width = new DataGridLength(1, DataGridLengthUnitType.SizeToHeader),

                // SortMemberPath 등을 PartId 기반으로 세팅할 수도 있음
                SortMemberPath = $"QuanDict[{part.PartId}]"
            };

            // (B) partInfo.Name 바뀔 때마다 이 열의 헤더 갱신
            part.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PartInfo.Name))
                {
                    newCol.Header = part.Name;
                }
            };

            WeekDataGrid.Columns.Add(newCol);
        }

        private void RemoveOneColumn(PartInfo part)
        {
            // PartInfo.Name 대신 PartId를 기준으로 찾아도 됨
            var col = WeekDataGrid.Columns
                .OfType<DataGridTextColumn>()
                .FirstOrDefault(c => c.SortMemberPath == $"QuanDict[{part.PartId}]");

            if (col != null)
            {
                WeekDataGrid.Columns.Remove(col);
            }
        }

        // DataGrid에서 셀 편집을 마칠 때 호출되는 이벤트 핸들러
        private void WeekDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var vm = DataContext as WeekPlanViewModel;
            if (vm != null)
            {
                // 셀 편집 완료 후 합계 재계산
                vm.RecalcSum();
            }
        }
    }
}
