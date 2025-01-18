using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HyunDaiINJ.Services; // IsoWeekCalculator

namespace HyunDaiINJ.Models.Plan
{
    public class WeekRow : INotifyPropertyChanged
    {
        public int Week { get; }
        public int Year { get; }

        // 합계 행 여부
        public bool IsSumRow { get; set; }

        private Dictionary<int, int> _quanDict = new Dictionary<int, int>();
        public Dictionary<int, int> QuanDict
        {
            get => _quanDict;
            set
            {
                _quanDict = value;
                OnPropertyChanged(nameof(QuanDict));
                OnPropertyChanged(nameof(RowSum));
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        // 합계 행이면 "합계", 아니면 ISO 주차 표시
        public string WeekDisplay
        {
            get
            {
                if (IsSumRow)
                {
                    return "합계";
                }
                else
                {
                    DateTime firstDay = IsoWeekCalculator.FirstDayOfIsoWeek(Year, Week);
                    return $"{Week}주차({firstDay:yyyy-MM-dd})";
                }
            }
        }

        // 동일하게, DisplayText도 합계/일반 분기
        public string DisplayText
        {
            get
            {
                if (IsSumRow) return "합계";
                return WeekDisplay;
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

        // 행 전체 합
        public int RowSum => _quanDict?.Values.Sum() ?? 0;

        // 하나의 생성자: 일반 행이면 year/week 기반으로 DateTime 계산,
        // 합계 행(isSumRow==true)이면 날짜 계산을 건너뛰기
        public WeekRow(int year, int w, bool isSumRow = false)
        {
            Year = year;
            Week = w;
            IsSumRow = isSumRow;

            if (!IsSumRow)
            {
                // 일반 행만 ISO 로직
                WeekStartDate = IsoWeekCalculator.FirstDayOfIsoWeek(year, w);
            }
            // 합계 행이면 생성자에서 DateTime 호출 안 함 (year=0이어도 예외 없음)
        }

        public void OnRowSumChanged()
        {
            OnPropertyChanged(nameof(RowSum));
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(WeekDisplay));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
