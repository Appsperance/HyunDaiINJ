using System;
using System.Collections.Generic;
using System.ComponentModel;
using HyunDaiINJ.Services; // IsoWeekCalculator

namespace HyunDaiINJ.Models.Plan
{
    public class WeekRow : INotifyPropertyChanged
    {
        public int Week { get; }
        public int Year { get; } // 예: 2023

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

        // "1주차(01-02)" 처럼 표시
        public string WeekDisplay
        {
            get
            {
                // 1) iso 주차의 월요일 날짜
                DateTime firstDay = IsoWeekCalculator.FirstDayOfIsoWeek(Year, Week);

                // 2) "1주차(01-02)" 포맷
                //    (원하시면 "YYYY-MM-dd" 등 다른 형식도 가능)
                return $"{Week}주차({firstDay:yyyy-MM-dd})";
            }
        }

        // ★ 추가: 주차 시작일 (DateTime)
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
