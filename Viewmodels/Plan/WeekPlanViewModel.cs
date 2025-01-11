using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Models.Plan;

public class WeekPlanViewModel : INotifyPropertyChanged
{
    public ObservableCollection<InjectionPlanModel> WeekPlans { get; set; }
    private string _partIdHeader = "제품 ID";

    public string PartIdHeader
    {
        get => _partIdHeader;
        set
        {
            if (_partIdHeader != value)
            {
                _partIdHeader = value;
                OnPropertyChanged(nameof(PartIdHeader));
            }
        }
    }

    private int _totalWeekQuan;
    public int TotalWeekQuan
    {
        get => _totalWeekQuan;
        set
        {
            if (_totalWeekQuan != value)
            {
                _totalWeekQuan = value;
                OnPropertyChanged(nameof(TotalWeekQuan));
            }
        }
    }

    public WeekPlanViewModel()
    {
        WeekPlans = new ObservableCollection<InjectionPlanModel>();

        // 52주차 초기화
        for (int i = 1; i <= 52; i++)
        {
            // 1) POCO DTO를 생성
            var dto = new InjectionPlanDTO
            {
                Week = i,
                WeekQuan = 0
            };

            // 2) DTO를 감싸는 Model 생성
            var model = new InjectionPlanModel(dto);

            // 3) 컬렉션에 추가
            WeekPlans.Add(model);
        }

        // 컬렉션 변경 구독 (새로운 Model 추가 시 합계 갱신)
        WeekPlans.CollectionChanged += (s, e) =>
        {
            UpdateTotalWeekQuan();
        };

        // 기존 아이템들의 변화 구독
        foreach (var model in WeekPlans)
        {
            // WeekQuan등이 바뀌면 갱신
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(InjectionPlanModel.WeekQuan))
                {
                    UpdateTotalWeekQuan();
                }
            };
        }

        // 초기 총합 계산
        UpdateTotalWeekQuan();
    }

    private void UpdateTotalWeekQuan()
    {
        // Model 내부에서 DTO 접근 가능하지만, 합계 계산은 Model에서 값을 꺼내 쓰면 됨
        TotalWeekQuan = WeekPlans.Sum(m => m.WeekQuan ?? 0);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
