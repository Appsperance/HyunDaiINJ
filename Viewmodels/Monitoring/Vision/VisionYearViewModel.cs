using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json; // or Newtonsoft.Json
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Models.Monitoring.Vision;

namespace HyunDaiINJ.ViewModels.Monitoring.Vision
{
    public class VisionYearViewModel : INotifyPropertyChanged
    {
        private readonly VisionNgModel _visionNgModel;

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

        // 차트 스크립트가 업데이트되면 UserControl에서 UpdateChart()를 호출하기 위한 이벤트
        public event Action ChartScriptUpdated;

        private void OnChartScriptUpdated()
        {
            ChartScriptUpdated?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public VisionYearViewModel()
        {
            _visionNgModel = new VisionNgModel();
        }

        /// <summary>
        /// 연간 데이터 로드 → ChartScript 생성
        /// </summary>
        public async Task LoadVisionNgDataYearAsync()
        {
            try
            {
                // 1) DAO 통해 DB에서 "연간" 데이터 조회
                var yearDataList = _visionNgModel.GetVisionNgDataYear();
                // yearDataList = List<VisionNgDTO> 
                //  여기에는 EXTRACT(YEAR ...)로 구한 year_num, ng_label, label_count 등이 들어 있을 것

                // 2) Chart.js config 생성
                var chartConfig = CreateChartConfig(yearDataList);

                // 3) JSON 직렬화
                string configJson = JsonSerializer.Serialize(chartConfig);

                // 4) ChartScript에 저장 후 이벤트
                ChartScript = configJson;
                OnChartScriptUpdated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadVisionNgDataYearAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// 연도별 데이터를 차트로 표현하는 예시
        /// - X축: 연도
        /// - 여러 ng_label이 각각 하나의 dataset
        ///   (또는 반대로 ng_label을 x축, 연도를 dataset으로 해도 됨)
        /// </summary>
        private object CreateChartConfig(List<VisionNgDTO> yearDataList)
        {
            // 가정: VisionNgDTO에 year_num, ng_label, label_count 들어 있음
            // 여기서는 "연도"를 X축으로, "ng_label"별 dataset을 생성하는 예시

            // 1) 연도별 라벨 목록
            var yearLabels = new HashSet<int>();
            // 2) ng_label 목록
            var labelGroups = new Dictionary<string, Dictionary<int, int>>();

            foreach (var item in yearDataList)
            {
                // 연도 int
                int yearNum = item.YearNumber; // 또는 item.YearNumber (DAO에서 매핑 필요)

                yearLabels.Add(yearNum);

                if (!labelGroups.ContainsKey(item.NgLabel))
                {
                    labelGroups[item.NgLabel] = new Dictionary<int, int>();
                }

                if (!labelGroups[item.NgLabel].ContainsKey(yearNum))
                {
                    labelGroups[item.NgLabel][yearNum] = 0;
                }

                labelGroups[item.NgLabel][yearNum] += item.LabelCount;
            }

            // X축: 정렬된 연도 목록
            var sortedYears = new List<int>(yearLabels);
            sortedYears.Sort();

            // 여러 dataset
            var datasets = new List<object>();

            // 컬러 팔레트
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
            // labelGroups: key=ng_label, value=Dictionary<yearNum, count>
            foreach (var kvp in labelGroups)
            {
                string ngLabel = kvp.Key;
                var yearCounts = kvp.Value;

                var dataList = new List<int>();
                foreach (var y in sortedYears)
                {
                    // 있다면 그 값, 없으면 0
                    dataList.Add(yearCounts.ContainsKey(y) ? yearCounts[y] : 0);
                }

                datasets.Add(new
                {
                    label = ngLabel, // dataset 이름
                    data = dataList,
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
                    labels = sortedYears, // X축: 2025, 2026, ...
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
                            text = "연간 불량 현황" // 차트 제목
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

        // 서버 통신 등 필요시
        public async Task ListenToServerAsync()
        {
            // 예: 실시간 업데이트
        }
    }
}
