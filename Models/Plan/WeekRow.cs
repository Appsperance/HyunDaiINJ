using System.Collections.Generic;
using System.ComponentModel;

namespace HyunDaiINJ.Models.Plan
{
    public class WeekRow : INotifyPropertyChanged
    {
        public int Week { get; }

        // PartId -> 수량
        private Dictionary<int, int> _quanDict = new Dictionary<int, int>();
        public Dictionary<int, int> QuanDict
        {
            get => _quanDict;
            set
            {
                if (_quanDict != value)
                {
                    _quanDict = value;
                    OnPropertyChanged(nameof(QuanDict));
                }
            }
        }

        public WeekRow(int w)
        {
            Week = w;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
