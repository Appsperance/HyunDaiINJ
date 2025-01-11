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
