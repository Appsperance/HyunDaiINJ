using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.Models.Plan
{
    public class InjectionPlanModel
    {
        private readonly InjectionPlanDTO _dto;

        public InjectionPlanModel(InjectionPlanDTO dto)
        {
            _dto = dto;
        }

        // 필요한 속성만 DTO에서 감싸서 노출
        public string? PartId
        {
            get => _dto.PartId;
            set
            {
                if (_dto.PartId != value)
                {
                    _dto.PartId = value;
                    OnPropertyChanged(nameof(PartId));
                }
            }
        }

        // (예시) 월/화/수/목/금/토/일 DailyQuan
        private int? _mondayQuan;
        public int? MondayQuan
        {
            get => _mondayQuan;
            set
            {
                if (_mondayQuan != value)
                {
                    _mondayQuan = value;
                    OnPropertyChanged(nameof(MondayQuan));
                }
            }
        }

        private int? _tuesdayQuan;
        public int? TuesdayQuan
        {
            get => _tuesdayQuan;
            set
            {
                if (_tuesdayQuan != value)
                {
                    _tuesdayQuan = value;
                    OnPropertyChanged(nameof(TuesdayQuan));
                }
            }
        }

        private int? _wednesdayQuan;
        public int? WednesdayQuan
        {
            get => _tuesdayQuan;
            set
            {
                if (_tuesdayQuan != value)
                {
                    _tuesdayQuan = value;
                    OnPropertyChanged(nameof(WednesdayQuan));
                }
            }
        }

        private int? _thursdayQuan;
        public int? ThursdayQuan
        {
            get => _thursdayQuan;
            set
            {
                if (_thursdayQuan != value)
                {
                    _thursdayQuan = value;
                    OnPropertyChanged(nameof(WednesdayQuan));
                }
            }
        }

        private int? _fridayQuan;
        public int? FridayQuan
        {
            get => _fridayQuan;
            set
            {
                if (_fridayQuan != value)
                {
                    _fridayQuan = value;
                    OnPropertyChanged(nameof(FridayQuan));
                }
            }
        }

        private int? _saturdayQuan;
        public int? SaturdayQuan
        {
            get => _saturdayQuan;
            set
            {
                if (_saturdayQuan != value)
                {
                    _saturdayQuan = value;
                    OnPropertyChanged(nameof(SaturdayQuan));
                }
            }
        }

        private int? _sundayQuan;
        public int? SundayQuan
        {
            get => _sundayQuan;
            set
            {
                if (_sundayQuan != value)
                {
                    _sundayQuan = value;
                    OnPropertyChanged(nameof(SundayQuan));
                }
            }
        }



        public int Week
        {
            get => _dto.Week;
            set
            {
                if (_dto.Week != value)
                {
                    _dto.Week = value;
                    OnPropertyChanged(nameof(Week));
                }
            }
        }

        public int? WeekQuan
        {
            get => _dto.WeekQuan;
            set
            {
                if (_dto.WeekQuan != value)
                {
                    _dto.WeekQuan = value;
                    OnPropertyChanged(nameof(WeekQuan));
                }
            }
        }

        // INotifyPropertyChanged 구현
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
