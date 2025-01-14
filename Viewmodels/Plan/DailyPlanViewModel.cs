﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.Models.Plan;
using HyunDaiINJ.Services;

namespace HyunDaiINJ.ViewModels.Plan
{
    public class DailyPlanViewModel : INotifyPropertyChanged
    {
        private readonly InjectionPlanDAO _dao;

        public DailyPlanViewModel()
        {
            _dao = new InjectionPlanDAO();

            // 1) 주차 계산
            int isoWeek = IsoWeekCalculator.GetIso8601WeekOfYear(DateTime.Today);
            CurrentWeekNumber = isoWeek;

            // 2) 주차별 데이터 로드
            _ = LoadWeekDataAsync(CurrentWeekNumber);

            // 3) 헤더: 오늘을 기준으로 "월(01-13)" 등 예시
            DateTime today = DateTime.Today;
            DateTime monday = today.AddDays(-(int)today.DayOfWeek + 1);
            MondayHeader = $"월({monday:MM/dd})";
            TuesdayHeader = $"화({monday.AddDays(1):MM/dd})";
            WednesdayHeader = $"수({monday.AddDays(2):MM/dd})";
            ThursdayHeader = $"목({monday.AddDays(3):MM/dd})";
            FridayHeader = $"금({monday.AddDays(4):MM/dd})";
            SaturdayHeader = $"토({monday.AddDays(5):MM/dd})";
            SundayHeader = $"일({monday.AddDays(6):MM/dd})";

            // DatePicker 기본 값
            _selectedDate = DateTime.Today;
        }

        // (A) 주차별 행들
        private ObservableCollection<DailyPlanModel> _dailyRows = new();
        public ObservableCollection<DailyPlanModel> DailyRows
        {
            get => _dailyRows;
            set
            {
                if (_dailyRows != value)
                {
                    _dailyRows = value;
                    OnPropertyChanged(nameof(DailyRows));
                }
            }
        }

        // (B) 현재 주차
        private int _currentWeekNumber;
        public int CurrentWeekNumber
        {
            get => _currentWeekNumber;
            set
            {
                if (_currentWeekNumber != value)
                {
                    _currentWeekNumber = value;
                    OnPropertyChanged(nameof(CurrentWeekNumber));
                }
            }
        }

        // (C) 월~일 헤더
        public string MondayHeader { get; set; }
        public string TuesdayHeader { get; set; }
        public string WednesdayHeader { get; set; }
        public string ThursdayHeader { get; set; }
        public string FridayHeader { get; set; }
        public string SaturdayHeader { get; set; }
        public string SundayHeader { get; set; }

        // (D) DatePicker와 바인딩할 속성
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                    // 날짜가 바뀌면 → 해당 날짜의 ISO 주차 계산 → DB 조회
                    if (_selectedDate.HasValue)
                    {
                        int newWeek = IsoWeekCalculator.GetIso8601WeekOfYear(_selectedDate.Value);
                        CurrentWeekNumber = newWeek;

                        // DB 다시 조회
                        _ = LoadWeekDataAsync(newWeek);
                    }
                }
            }
        }

        /// <summary>
        /// DB에서 해당 주차의 데이터를 가져와서 DailyRows에 세팅
        /// </summary>
        private async Task LoadWeekDataAsync(int isoWeek)
        {
            try
            {
                Console.WriteLine($"[VM] LoadWeekDataAsync => week={isoWeek}");
                var list = await _dao.GetPlansByWeekAsync(isoWeek);

                DailyRows = new ObservableCollection<DailyPlanModel>(list);

                Console.WriteLine($"[VM] DB 로드 완료. 건수={list.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VM] LoadWeekDataAsync 오류: {ex.Message}");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
    }
}