using System.ComponentModel;

namespace HyunDaiINJ.Models.Plan
{
    /// <summary>
    /// DataGrid에 표시할 DailyPlan (행) 모델
    /// </summary>
    public class DailyPlanModel : INotifyPropertyChanged
    {
        private string _partId;
        private int _week;
        private int _weekQuan;

        // 요일별 수량 (원하는 만큼 추가)
        private int _monQuan;
        private int _tueQuan;
        private int _wedQuan;
        private int _thuQuan;
        private int _friQuan;
        private int _satQuan;
        private int _sunQuan;

        public string PartId
        {
            get => _partId;
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
        public int WeekQuan
        {
            get => _weekQuan;
            set
            {
                if (_weekQuan != value)
                {
                    _weekQuan = value;
                    OnPropertyChanged(nameof(WeekQuan));
                    OnPropertyChanged(nameof(PartIdWithTotal));
                }
            }
        }
        public int Week
        {
            get => _week;
            set
            {
                if (_week != value)
                {
                    _week = value;
                    OnPropertyChanged(nameof(Week));
                }
            }
        }

        // 월~일까지 숫자를 입력받을 속성
        public int MonQuan
        {
            get => _monQuan;
            set
            {
                if (_monQuan != value)
                {
                    _monQuan = value;
                    OnPropertyChanged(nameof(MonQuan));
                    OnPropertyChanged(nameof(PartIdWithTotal)); // 합계갱신
                }
            }
        }

        public int TueQuan
        {
            get => _tueQuan;
            set
            {
                if (_tueQuan != value)
                {
                    _tueQuan = value;
                    OnPropertyChanged(nameof(TueQuan));
                    OnPropertyChanged(nameof(PartIdWithTotal));
                }
            }
        }

        public int WedQuan
        {
            get => _wedQuan;
            set
            {
                if (_wedQuan != value)
                {
                    _wedQuan = value;
                    OnPropertyChanged(nameof(WedQuan));
                    OnPropertyChanged(nameof(PartIdWithTotal));
                }
            }
        }

        public int ThuQuan
        {
            get => _thuQuan;
            set
            {
                if (_thuQuan != value)
                {
                    _thuQuan = value;
                    OnPropertyChanged(nameof(ThuQuan));
                    OnPropertyChanged(nameof(PartIdWithTotal));
                }
            }
        }

        public int FriQuan
        {
            get => _friQuan;
            set
            {
                if (_friQuan != value)
                {
                    _friQuan = value;
                    OnPropertyChanged(nameof(FriQuan));
                    OnPropertyChanged(nameof(PartIdWithTotal));
                }
            }
        }

        public int SatQuan
        {
            get => _satQuan;
            set
            {
                if (_satQuan != value)
                {
                    _satQuan = value;
                    OnPropertyChanged(nameof(SatQuan));
                    OnPropertyChanged(nameof(PartIdWithTotal));
                }
            }
        }

        public int SunQuan
        {
            get => _sunQuan;
            set
            {
                if (_sunQuan != value)
                {
                    _sunQuan = value;
                    OnPropertyChanged(nameof(SunQuan));
                    OnPropertyChanged(nameof(PartIdWithTotal));
                }
            }
        }

        /// <summary>
        /// 예) "AAAA(350)" 처럼 표시
        /// (월~일 합계를 자동 계산)
        /// </summary>
        public string PartIdWithTotal
            => $"{PartId}({WeekQuan})";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
