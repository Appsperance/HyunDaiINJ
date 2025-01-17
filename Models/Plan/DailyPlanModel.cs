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

        /// <summary>
        /// "월" 프로퍼티: 입력 값이 너무 커서 QtyWeekly가 음수가 되면 0으로 설정
        /// </summary>
        public int MonQuan
        {
            get => _monQuan;
            set
            {
                if (_monQuan != value)
                {
                    int oldVal = _monQuan;
                    _monQuan = value;
                    int delta = _monQuan - oldVal;
                    QtyWeekly -= delta;

                    if (QtyWeekly < 0)
                    {
                        // 복원 로직 - 0으로 만들기
                        int revert = _monQuan; // 새로 입력한 값
                        _monQuan = 0;         // 요일을 0으로
                        QtyWeekly += revert;  // 이미 한번 빼놨으므로 다시 되돌림

                        System.Windows.MessageBox.Show(
                            "남은 수량이 없습니다.",
                            "주의",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning
                        );
                        return;
                    }

                    OnPropertyChanged(nameof(MonQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        /// <summary>
        /// "화"
        /// </summary>
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

                    if (QtyWeekly < 0)
                    {
                        int revert = _tueQuan;
                        _tueQuan = 0;
                        QtyWeekly += revert;

                        System.Windows.MessageBox.Show("남은 수량이 없습니다.");
                        return;
                    }

                    OnPropertyChanged(nameof(TueQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        /// <summary>
        /// "수"
        /// </summary>
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

                    if (QtyWeekly < 0)
                    {
                        int revert = _wedQuan;
                        _wedQuan = 0;
                        QtyWeekly += revert;

                        System.Windows.MessageBox.Show("남은 수량이 없습니다.");
                        return;
                    }

                    OnPropertyChanged(nameof(WedQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        /// <summary>
        /// "목"
        /// </summary>
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

                    if (QtyWeekly < 0)
                    {
                        int revert = _thuQuan;
                        _thuQuan = 0;
                        QtyWeekly += revert;

                        System.Windows.MessageBox.Show("남은 수량이 없습니다.");
                        return;
                    }

                    OnPropertyChanged(nameof(ThuQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        /// <summary>
        /// "금"
        /// </summary>
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

                    if (QtyWeekly < 0)
                    {
                        int revert = _friQuan;
                        _friQuan = 0;
                        QtyWeekly += revert;

                        System.Windows.MessageBox.Show("남은 수량이 없습니다.");
                        return;
                    }

                    OnPropertyChanged(nameof(FriQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        /// <summary>
        /// "토"
        /// </summary>
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

                    if (QtyWeekly < 0)
                    {
                        int revert = _satQuan;
                        _satQuan = 0;
                        QtyWeekly += revert;

                        System.Windows.MessageBox.Show("남은 수량이 없습니다.");
                        return;
                    }

                    OnPropertyChanged(nameof(SatQuan));
                    OnPropertyChanged(nameof(DailyTotal));
                }
            }
        }

        /// <summary>
        /// "일"
        /// </summary>
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

                    if (QtyWeekly < 0)
                    {
                        int revert = _sunQuan;
                        _sunQuan = 0;
                        QtyWeekly += revert;

                        System.Windows.MessageBox.Show("남은 수량이 없습니다.");
                        return;
                    }

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
