﻿using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Text.Json;
using HyunDaiINJ.Models.Monitoring.ThirdSection;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.ViewModels.Monitoring.ThirdSection
{
    public class VisionNgViewModel
    {
        private readonly VisionNgModel _visionNgModel;

        // Chart.js용 데이터
        public List<VisionNgDTO> NgSummaryData { get; private set; }

        // DataGrid용 데이터
        public ObservableCollection<VisionNgDTO> NgDetailedData { get; private set; }

        public VisionNgViewModel()
        {
            _visionNgModel = new VisionNgModel();
            LoadData();
        }

        // 데이터를 로드하는 메서드
        private void LoadData()
        {
            try
            {
                // VisionNgModel을 통해 데이터를 가져옴
                NgSummaryData = _visionNgModel.GetVisionNgData();      // 요약 데이터
                NgDetailedData = new ObservableCollection<VisionNgDTO>(_visionNgModel.GetVisionNgDataAll()); // 상세 데이터
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                throw;
            }
        }

    }
}
