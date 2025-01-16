using System;
using System.Collections.Generic;
using System.Text.Json; // 또는 Newtonsoft.Json
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Models.Monitoring.Vision;

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class VisionDailyViewModel : INotifyPropertyChanged
    {
        private readonly VisionNgModel _visionNgModel;

        // 일간 데이터를 담을 컬렉션 (옵션)
        private List<VisionNgDTO> _dailyDataList;
        public List<VisionNgDTO> DailyDataList
        {
            get => _dailyDataList;
            set
            {
                _dailyDataList = value;
                OnPropertyChanged();
            }
        }

        // 차트에 넣을 JS config(JSON) 스크립트
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

        // 차트가 업데이트되었음을 알리는 이벤트 (UserControl 등에서 구독 가능)
        public event Action ChartScriptUpdated;
        private void OnChartScriptUpdated() => ChartScriptUpdated?.Invoke();

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public VisionDailyViewModel()
        {
            _visionNgModel = new VisionNgModel();
            DailyDataList = new List<VisionNgDTO>();
        }

        /// <summary>
        /// DB나 서버에서 "오늘 날짜" 기준 데이터를 불러와, 라벨별로 표시
        /// </summary>
        public async Task LoadVisionNgDataDailyAsync()
        {
            try
            {
                // (1) DB/서버에서 일간 데이터 조회
                //     예: VisionNgModel.GetVisionNgDataDaily();
                //     여기서는 Model(DAO) 호출로 List<VisionNgDTO> 가져온다고 가정
                var dailyData = _visionNgModel.GetVisionNgDataDaily();
                // dailyData가 각각 (NgLabel, LabelCount) 등을 포함한다고 가정

                if (dailyData == null || dailyData.Count == 0)
                {
                    Console.WriteLine("[LoadVisionNgDataDailyAsync] no daily data");
                    return;
                }

                DailyDataList = dailyData; // 바인딩 등에서 쓸 수 있음

                // (2) Chart.js config 생성
                var chartConfig = CreateChartConfig(dailyData);

                // (3) JSON 직렬화
                string configJson = JsonSerializer.Serialize(chartConfig);

                // (4) ChartScript에 저장 → 이벤트
                ChartScript = configJson;
                OnChartScriptUpdated();

                // Debug
                Console.WriteLine("[LoadVisionNgDataDailyAsync] ChartScript updated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadVisionNgDataDailyAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// 일간 데이터(dailyDataList)를 x축 하나(예: 오늘), 여러 개 ng_label로 dataset 구성
        /// </summary>
        private object CreateChartConfig(List<VisionNgDTO> dailyDataList)
        {
            // X축은 "오늘" 1칸만
            var xLabels = new[] { DateTime.Today.ToString("yyyy-MM-dd") };

            // dataset: 라벨별로 1개씩
            var datasets = new List<object>();

            var colorPalette = new List<string>
            {
                "rgba(75, 192, 192, 0.7)",
                "rgba(255, 99, 132, 0.7)",
                "rgba(54, 162, 235, 0.7)",
                "rgba(255, 206, 86, 0.7)",
                "rgba(153, 102, 255, 0.7)",
                "rgba(255, 159, 64, 0.7)"
            };

            int colorIndex = 0;
            foreach (var item in dailyDataList)
            {
                // 예: 라벨=item.NgLabel, data={ item.LabelCount }
                datasets.Add(new
                {
                    label = item.NgLabel,
                    data = new[] { item.LabelCount },
                    backgroundColor = colorPalette[colorIndex % colorPalette.Count]
                });
                colorIndex++;
            }

            // Chart.js config
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
                            text = "오늘 불량 (일간)"
                        }
                    },
                    scales = new
                    {
                        y = new
                        {
                            beginAtZero = true,
                            title = new
                            {
                                display = true,
                                text = "건수"
                            }
                        }
                    }
                }
            };

            return chartConfig;
        }
    }
}
