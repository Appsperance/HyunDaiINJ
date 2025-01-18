using System;
using System.Collections.Generic;
using System.ComponentModel;
using HyunDaiINJ.Services; // IsoWeekCalculator

namespace HyunDaiINJ.Models.Plan
{
    public class WeekRow : INotifyPropertyChanged
    {
        public int Week { get; }
        public int Year { get; }

        // ★ Dictionary의 Key = part.Name (string), Value = 수량(int)
        private Dictionary<int, int> _quanDict = new Dictionary<int, int>();
        public Dictionary<int, int> QuanDict
        {
            get => _quanDict;
            set
            {
                _quanDict = value;
                OnPropertyChanged(nameof(QuanDict));
            }
        }

        // "3주차(2023-01-16)" 같은 식으로 표시
        public string WeekDisplay
        {
            get
            {
                DateTime firstDay = IsoWeekCalculator.FirstDayOfIsoWeek(Year, Week);
                return $"{Week}주차({firstDay:yyyy-MM-dd})";
            }
        }

        private DateTime _weekStartDate;
        public DateTime WeekStartDate
        {
            get => _weekStartDate;
            set
            {
                if (_weekStartDate != value)
                {
                    _weekStartDate = value;
                    OnPropertyChanged(nameof(WeekStartDate));
                }
            }
        }

        public WeekRow(int year, int w)
        {
            Year = year;
            Week = w;
            WeekStartDate = IsoWeekCalculator.FirstDayOfIsoWeek(year, w);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}