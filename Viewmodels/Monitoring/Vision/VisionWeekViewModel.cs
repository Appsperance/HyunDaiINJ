using System;
using System.Collections.Generic;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Models.Monitoring.Vision;

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class VisionWeekViewModel : INotifyPropertyChanged
    {
        private readonly VisionNgModel _visionNgModel;
        private readonly List<string> _colorPalette;
        private string _chartScript;

        public event Action ChartScriptUpdated;
        public event PropertyChangedEventHandler PropertyChanged;

        // (옵션) 주간 DTO 리스트를 보관하려면 추가
        // public List<VisionNgDTO> WeekDataList { get; set; }

        public string ChartScript
        {
            get => _chartScript;
            private set
            {
                _chartScript = value;
                OnPropertyChanged();
                ChartScriptUpdated?.Invoke(); // 차트 스크립트 변경 시점 알림
            }
        }

        public VisionWeekViewModel()
        {
            _visionNgModel = new VisionNgModel();

            // 파스텔톤 색상 팔레트 (차트에 사용)
            _colorPalette = new List<string>
            {
                "rgba(75, 192, 192, 1)",
                "rgba(255, 99, 132, 1)",
                "rgba(54, 162, 235, 1)",
                "rgba(255, 206, 86, 1)",
                "rgba(153, 102, 255, 1)",
                "rgba(255, 159, 64, 1)"
            };
        }

        // (1) 주간 데이터 로드
        public async Task LoadVisionNgDataWeekAsync()
        {
            try
            {
                // 예: DAO -> (WeekNumber, NgLabel, LabelCount) 리스트
                var visionNgDataWeek = await Task.Run(() => _visionNgModel.GetVisionNgDataWeek());

                if (visionNgDataWeek == null || visionNgDataWeek.Count == 0)
                {
                    ChartScript = GenerateEmptyChartScript();
                    return;
                }

                // (2) Chart.js config JSON 생성
                ChartScript = GenerateChartScript(visionNgDataWeek);

                // (옵션) WeekDataList = visionNgDataWeek;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"주간 Ng_Label 데이터 에러 : {ex.Message}");
            }
        }

        // (2) ChartScript 생성: GroupBy(ngLabel, weekNumber) -> Sum(labelCount)
        private string GenerateChartScript(List<VisionNgDTO> visionNgDataWeek)
        {
            // groupedData: key=ng_label, val=(Dictionary<week -> sum(labelCount)>)
            var groupedData = new Dictionary<string, Dictionary<int, int>>();
            var weekLabels = new HashSet<int>();

            // Grouping
            foreach (var data in visionNgDataWeek)
            {
                if (!groupedData.ContainsKey(data.NgLabel))
                {
                    groupedData[data.NgLabel] = new Dictionary<int, int>();
                }

                if (!groupedData[data.NgLabel].ContainsKey(data.WeekNumber))
                {
                    groupedData[data.NgLabel][data.WeekNumber] = 0;
                }

                groupedData[data.NgLabel][data.WeekNumber] += data.LabelCount;
                weekLabels.Add(data.WeekNumber);
            }

            // week 정렬
            var sortedWeeks = new List<int>(weekLabels);
            sortedWeeks.Sort();

            // dataset
            var datasets = new List<object>();
            int colorIndex = 0;

            foreach (var kvp in groupedData)
            {
                string label = kvp.Key; // ngLabel
                var weekMap = kvp.Value; // Dictionary< weekNumber -> totalCount >
                var dataPoints = new List<int>();

                foreach (var w in sortedWeeks)
                {
                    dataPoints.Add(weekMap.ContainsKey(w) ? weekMap[w] : 0);
                }

                // 배경색
                string color = _colorPalette[colorIndex % _colorPalette.Count].Replace("1)", "0.6)");
                datasets.Add(new
                {
                    label,
                    data = dataPoints,
                    backgroundColor = color
                });
                colorIndex++;
            }

            // X축: "Week 3", "Week 4" 등
            var weekLabelsStr = new List<string>();
            foreach (var w in sortedWeeks)
            {
                weekLabelsStr.Add($"Week {w}");
            }

            // Chart.js config 객체
            var config = new
            {
                type = "bar",
                data = new
                {
                    labels = weekLabelsStr,
                    datasets = datasets
                },
                options = new
                {
                    responsive = true,
                    plugins = new { },
                    scales = new
                    {
                        x = new { stacked = true },
                        y = new { stacked = true }
                    }
                }
            };

            // 직렬화 -> JSON
            var json = JsonSerializer.Serialize(config);
            return json;
        }

        private string GenerateEmptyChartScript()
        {
            var config = new
            {
                type = "bar",
                data = new
                {
                    labels = new[] { "No Data" },
                    datasets = new object[]
                    {
                        new
                        {
                            label = "Empty",
                            data = new[] {0},
                            backgroundColor = "rgba(200,200,200,0.5)"
                        }
                    }
                },
                options = new
                {
                    responsive = true,
                    plugins = new
                    {
                        title = new { display = true, text = "No Data" }
                    }
                }
            };
            return JsonSerializer.Serialize(config);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
