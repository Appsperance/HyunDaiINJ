using System;
using System.Threading.Tasks;
using System.Text.Json; // 또는 Newtonsoft.Json
using HyunDaiINJ.Models.Monitoring.Vision;
using System.Collections.Generic;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.ViewModels.Main;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class VisionDailyViewModel : INotifyPropertyChanged // BaseViewModel은 INotifyPropertyChanged 구현 가정
    {
        private readonly VisionNgModel _visionNgModel;
        public event PropertyChangedEventHandler PropertyChanged;

        // WebView2에 주입할 차트 스크립트 (JSON 형태)
        private string _chartScript;
        public string ChartScript
        {
            get => _chartScript;
            set
            {
                _chartScript = value;
                OnPropertyChanged();
            }
        }

        // 이벤트: 차트 스크립트 업데이트 시 통지 → UserControl에서 UpdateChart() 호출
        public event Action ChartScriptUpdated;
        private void OnChartScriptUpdated()
        {
            ChartScriptUpdated?.Invoke();
        }

        public VisionDailyViewModel()
        {
            _visionNgModel = new VisionNgModel();
        }

        // 일간 데이터 로드 후 ChartScript 생성
        public async Task LoadVisionNgDataDailyAsync()
        {
            try
            {
                // (1) DAO 통해 DB에서 일간 데이터 조회
                var dailyDataList = _visionNgModel.GetVisionNgDataDaily();
                // dailyDataList = List<VisionNgDTO>, 여기서 ng_label, label_count 등을 포함

                // (2) Chart.js config 생성
                var chartConfig = CreateChartConfig(dailyDataList);

                // (3) C# 객체 → JSON (Chart.js에서 해석할 config)
                // Newtonsoft나 System.Text.Json 중 편한 걸 쓰면 됨
                string configJson = JsonSerializer.Serialize(chartConfig);

                // (4) ChartScript에 저장 후 이벤트
                ChartScript = configJson;
                OnChartScriptUpdated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadVisionNgDataDailyAsync error: {ex.Message}");
            }
        }

        // 차트 구성을 만드는 예시 (Bar 차트)
        private object CreateChartConfig(List<VisionNgDTO> dailyDataList)
        {
            // 1) X축에 표시할 항목 (일간이므로 "오늘" 1개만)
            //    날짜를 문자열로 넣어도 됩니다. 예: DateTime.Today.ToShortDateString()
            var xLabels = new[] { DateTime.Today.ToShortDateString() };

            // 2) 여러 dataset(각 ng_label별로 1개)을 담을 리스트
            var datasets = new List<object>();

            // 3) 색상 팔레트 준비 (필요한만큼 늘리거나, 무작위 생성 가능)
            var colorPalette = new List<string>
    {
        "rgba(75, 192, 192, 0.7)",  // 청록
        "rgba(255, 99, 132, 0.7)",  // 빨강
        "rgba(54, 162, 235, 0.7)",  // 파랑
        "rgba(255, 206, 86, 0.7)",  // 노랑
        "rgba(153, 102, 255, 0.7)", // 보라
        "rgba(255, 159, 64, 0.7)",  // 주황
        // 필요하면 더 추가
    };

            // 4) dailyDataList를 돌면서,
            //    ng_label마다 dataset을 하나씩 만든다.
            int colorIndex = 0;
            foreach (var item in dailyDataList)
            {
                var dataset = new
                {
                    label = item.NgLabel,                  // 예: "cd", "cdh", ...
                    data = new[] { item.LabelCount },      // 이 라벨의 값은 단 하나
                    backgroundColor = colorPalette[colorIndex % colorPalette.Count]
                };
                datasets.Add(dataset);
                colorIndex++;
            }

            // 5) Chart.js config 생성:
            //    X축 labels = { "오늘" }
            //    datasets = 라벨별로 구성된 여러개의 dataset
            var chartConfig = new
            {
                type = "bar",
                data = new
                {
                    labels = xLabels,
                    datasets = datasets
                },
                options = new
                {
                    responsive = true,
                    plugins = new
                    {
                        legend = new { position = "top" },
                        title = new
                        {
                            display = true,
                            text = "오늘 불량" // 차트 제목
                        }
                    },
                    scales = new
                    {
                        x = new { stacked = false },
                        y = new { stacked = false }
                    }
                }
            };

            return chartConfig;
        }


        // 서버 통신 / 실시간 업데이트 등 필요시 이 안에서 처리
        public async Task ListenToServerAsync()
        {
            // 예: MQTT / Socket 구독 등을 통해 실시간 데이터 받고,
            //     ChartScript 재생성, OnChartScriptUpdated() 호출
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
