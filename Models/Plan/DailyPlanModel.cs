using System.ComponentModel;

namespace HyunDaiINJ.Models.Plan
{
    public class DailyPlanModel : INotifyPropertyChanged
    {
        private string? _partId;
        private int _isoWeek;
        private int _qtyWeekly;

        // 요일별 수량
        private int _monQuan;
        private int _tueQuan;
        private int _wedQuan;
        private int _thuQuan;
        private int _friQuan;
        private int _satQuan;
        private int _sunQuan;

        // 월~일 합계
        public int DailyTotal
            => MonQuan + TueQuan + WedQuan + ThuQuan + FriQuan + SatQuan + SunQuan;

        public string PartId
        {
            get => _partId ?? "";
            set
            {
                if (_partId != value)
                {
                    _partId = value;
                    OnPropertyChanged(nameof(PartId));
                    OnPropertyChanged(nameof(PartIdWithTotal));
                }
            }
        }

        public int IsoWeek
        {
            get => _isoWeek;
            set
            {
                if (_isoWeek != value)
                {
                    _isoWeek = value;
                    OnPropertyChanged(nameof(IsoWeek));
                }
            }
        }

        public int QtyWeekly
        {
            get => _qtyWeekly;
            set
            {
                if (_qtyWeekly != value)
                {
                    _qtyWeekly = value;
                    OnPropertyChanged(nameof(QtyWeekly));
                    OnPropertyChanged(nameof(PartIdWithTotal));
                }
            }
        }

        // ====================
        // MonQuan 예시 (setter에서 QtyWeekly 증감)
        // ====================
        public int MonQuan
        {
            get => _monQuan;
            set
            {
                if (_monQuan != value)
                {
                    int oldVal = _monQuan;
                    _monQuan = value;

                    int delta = _monQuan - oldVal;  // 증감량
                    QtyWeekly -= delta;             // QtyWeekly 줄이거나 늘리기
                    CheckQtyWeeklyZero();

                    OnPropertyChanged(nameof(MonQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        // 화요일
        public int TueQuan
        {
            get => _tueQuan;
            set
            {
                if (_tueQuan != value)
                {
                    int oldVal = _tueQuan;
                    _tueQuan = value;

                    int delta = _tueQuan - oldVal;
                    QtyWeekly -= delta;
                    CheckQtyWeeklyZero();

                    OnPropertyChanged(nameof(TueQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        // 수요일
        public int WedQuan
        {
            get => _wedQuan;
            set
            {
                if (_wedQuan != value)
                {
                    int oldVal = _wedQuan;
                    _wedQuan = value;

                    int delta = _wedQuan - oldVal;
                    QtyWeekly -= delta;
                    CheckQtyWeeklyZero();

                    OnPropertyChanged(nameof(WedQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        // 목요일
        public int ThuQuan
        {
            get => _thuQuan;
            set
            {
                if (_thuQuan != value)
                {
                    int oldVal = _thuQuan;
                    _thuQuan = value;

                    int delta = _thuQuan - oldVal;
                    QtyWeekly -= delta;
                    CheckQtyWeeklyZero();

                    OnPropertyChanged(nameof(ThuQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        // 금요일
        public int FriQuan
        {
            get => _friQuan;
            set
            {
                if (_friQuan != value)
                {
                    int oldVal = _friQuan;
                    _friQuan = value;

                    int delta = _friQuan - oldVal;
                    QtyWeekly -= delta;
                    CheckQtyWeeklyZero();

                    OnPropertyChanged(nameof(FriQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        // 토요일
        public int SatQuan
        {
            get => _satQuan;
            set
            {
                if (_satQuan != value)
                {
                    int oldVal = _satQuan;
                    _satQuan = value;

                    int delta = _satQuan - oldVal;
                    QtyWeekly -= delta;
                    CheckQtyWeeklyZero();

                    OnPropertyChanged(nameof(SatQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        // 일요일
        public int SunQuan
        {
            get => _sunQuan;
            set
            {
                if (_sunQuan != value)
                {
                    int oldVal = _sunQuan;
                    _sunQuan = value;

                    int delta = _sunQuan - oldVal;
                    QtyWeekly -= delta;
                    CheckQtyWeeklyZero();

                    OnPropertyChanged(nameof(SunQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        /// <summary>
        /// 예: "AAAA(350)" 식으로 표시
        /// </summary>
        public string PartIdWithTotal
            => $"{PartId}({_qtyWeekly})";

        // 0 이하 되면 메시지
        private void CheckQtyWeeklyZero()
        {
            if (_qtyWeekly <= 0)
            {
                _qtyWeekly = 0;
                System.Windows.MessageBox.Show(
                    $"{PartId}의 남은 수량이 없습니다.",
                    "주의",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                OnPropertyChanged(nameof(QtyWeekly));
                OnPropertyChanged(nameof(PartIdWithTotal));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
