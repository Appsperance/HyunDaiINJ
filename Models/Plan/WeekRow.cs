using System.ComponentModel;

namespace MyApp.Models.Plan
{
    /// <summary>
    /// 한 주(Week)에 대해 Part1~4의 수량을 모두 갖는 행
    /// </summary>
    public class WeekRow : INotifyPropertyChanged
    {
        private int _week;
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

        private int _quanPart1;
        public int QuanPart1
        {
            get => _quanPart1;
            set
            {
                if (_quanPart1 != value)
                {
                    _quanPart1 = value;
                    OnPropertyChanged(nameof(QuanPart1));
                }
            }
        }

        private int _quanPart2;
        public int QuanPart2
        {
            get => _quanPart2;
            set
            {
                if (_quanPart2 != value)
                {
                    _quanPart2 = value;
                    OnPropertyChanged(nameof(QuanPart2));
                }
            }
        }

        private int _quanPart3;
        public int QuanPart3
        {
            get => _quanPart3;
            set
            {
                if (_quanPart3 != value)
                {
                    _quanPart3 = value;
                    OnPropertyChanged(nameof(QuanPart3));
                }
            }
        }

        private int _quanPart4;
        public int QuanPart4
        {
            get => _quanPart4;
            set
            {
                if (_quanPart4 != value)
                {
                    _quanPart4 = value;
                    OnPropertyChanged(nameof(QuanPart4));
                }
            }
        }

        public WeekRow(int week)
        {
            _week = week;
            _quanPart1 = 0;
            _quanPart2 = 0;
            _quanPart3 = 0;
            _quanPart4 = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
