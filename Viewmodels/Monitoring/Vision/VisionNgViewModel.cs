using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Models.Monitoring.Vision;

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class VisionNgViewModel
    {
        private readonly VisionNgModel _model;

        // 일간, 주간, 연간 데이터 각각 보관
        public List<VisionNgDTO> DailyData { get; private set; }
        public List<VisionNgDTO> WeekData { get; private set; }
        public List<VisionNgDTO> YearData { get; private set; }

        // DataGrid용 데이터
        public List<VisionNgDTO> NgSummaryData { get; private set; }   // (ItemsControl)
        public ObservableCollection<VisionNgDTO> NgDetailedData { get; private set; } // (DataGrid)

        public VisionNgViewModel()
        {
            _model = new VisionNgModel();
            LoadAllData();
        }

        // 데이터를 로드하는 메서드
        private void LoadAllData()
        {
            // 여기서 한꺼번에 로드
            DailyData = _model.GetVisionNgDataDaily();   // 일간 
            WeekData = _model.GetVisionNgDataWeek();    // 주간
            YearData = _model.GetVisionNgDataYear();    // 연간

            NgSummaryData = _model.GetVisionNgData();     // 기존 요약
            var detailedData = _model.GetVisionNgDataAll(); // 전체
            NgDetailedData = new ObservableCollection<VisionNgDTO>(detailedData);
        }
    }
}


