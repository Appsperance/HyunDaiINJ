using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HyunDaiINJ.ViewModels.Plan;
using HyunDaiINJ.Models.Plan;
using HyunDaiINJ.ViewModels;
using System.Windows.Controls.Primitives;

namespace HyunDaiINJ.Views.Plan.Controls.Week
{
    public partial class WeekPlanControl : UserControl
    {
        private bool _isWeekColumnAdded = false;
        private bool _isTotalColAdded = false;

        public WeekPlanControl()
        {
            InitializeComponent();
            Loaded += WeekPlanControl_Loaded;
        }

        private void WeekPlanControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as WeekPlanViewModel;
            if (vm == null) return;

            vm.PartInfoList.CollectionChanged += PartInfoList_CollectionChanged;

            EnsureWeekColumn();

            foreach (var part in vm.PartInfoList)
            {
                AddOneColumn(part);
            }

            // (추가) '계획 총합' 열을 마지막에 추가
            EnsureTotalColumn();
        }
        private void EnsureTotalColumn()
        {
            if (_isTotalColAdded) return;

            var sumColumn = new DataGridTextColumn
            {
                Header = "계획 총합",
                Binding = new Binding("RowSum")
                {
                    Mode = BindingMode.OneWay
                },
                Width = new DataGridLength(1, DataGridLengthUnitType.SizeToHeader),
                IsReadOnly = true
            };

            WeekDataGrid.Columns.Add(sumColumn);
            _isTotalColAdded = true;
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
                Binding = new Binding("WeekDisplay"),
                Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells),
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

             // (A) 표시 모드(TextBlock)에서 가운데 정렬
    var textBlockStyle = new Style(typeof(TextBlock));
    textBlockStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
    // (선택) 수직 정렬을 맞추고 싶다면 VerticalAlignment도 가능

    // (B) 편집 모드(TextBox)에서 가운데 정렬
    var textBoxStyle = new Style(typeof(TextBox));
    textBoxStyle.Setters.Add(new Setter(TextBox.TextAlignmentProperty, TextAlignment.Center));
    // (선택) VerticalContentAlignment도 가능

    newCol.ElementStyle = textBlockStyle;       // 읽기 전용(표시) 상태일 때 텍스트 정렬
    newCol.EditingElementStyle = textBoxStyle;  // 편집 상태일 때 텍스트 정렬


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