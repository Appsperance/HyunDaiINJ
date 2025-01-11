using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using HyunDaiINJ.DATA.DTO;

public class WeekPlanViewModel : INotifyPropertyChanged
{
    public ObservableCollection<InjectionPlanDTO> WeekPlans { get; set; }
    private string _partIdHeader = "제품 ID"; // 초기값 설정

    public string PartIdHeader
    {
        get => _partIdHeader;
        set
        {
            if (_partIdHeader != value)
            {
                _partIdHeader = value;
                Console.WriteLine($"PartIdHeader changed to: {_partIdHeader}");
                OnPropertyChanged(nameof(PartIdHeader));
            }
        }
    }

    public WeekPlanViewModel()
    {
        WeekPlans = new ObservableCollection<InjectionPlanDTO>();

        // 52주차 데이터 초기화
        for (int i = 1; i <= 52; i++)
        {
            WeekPlans.Add(new InjectionPlanDTO
            {
                Week = i,
            });
        }

    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
