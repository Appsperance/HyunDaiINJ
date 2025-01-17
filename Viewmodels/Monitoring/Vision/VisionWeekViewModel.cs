using System;
using System.Collections.Generic;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;
using System.Globalization; // 주차 계산 시 사용
using System.Linq;

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class VisionWeekViewModel : INotifyPropertyChanged
    {
        // API를 통해 데이터를 가져온다고 가정
        private readonly MSDApi _api;

        private readonly List<string> _colorPalette;
        private string _chartScript;

        public event Action ChartScriptUpdated;
        public event PropertyChangedEventHandler PropertyChanged;

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
            _api = new MSDApi();

            // 차트 색상 팔레트
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

        /// <summary>
        /// 주간 데이터 로드 (API 호출 → WeekNumber 계산 → ChartScript 생성)
        /// </summary>
        public async Task LoadVisionNgDataWeekAsync()
        {
            try
            {
                var lineIds = new List<string> { "vp01", "vp02", "vp03", "vp04", "vp05" };
                int offset = 0;
                int count = 500;

                // API 호출
                var dtoList = await _api.GetNgImagesAsync(lineIds, offset, count);

                if (dtoList == null || dtoList.Count == 0)
                {
                    ChartScript = GenerateEmptyChartScript();
                    return;
                }

                // 주차 계산
                foreach (var d in dtoList)
                {
                    if (!string.IsNullOrEmpty(d.DateTime))
                    {
                        if (DateTime.TryParse(d.DateTime, out var parsedDt))
                        {
                            d.WeekNumber = GetWeekNumber(parsedDt);
                        }
                    }
                }

                // (선택) 현재 주만 필터링하거나, 특정 기간만 보고 싶다면 여기서 dtoList를 걸러낼 수 있음
                // 예: int currentWeek = GetWeekNumber(DateTime.Now);
                // dtoList = dtoList.Where(x => x.WeekNumber == currentWeek).ToList();

                // 주차+라벨 그룹화 → Count() 후, ChartScript 생성
                ChartScript = GenerateChartScript(dtoList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"주간 Ng_Label 데이터 에러 : {ex.Message}");
                ChartScript = GenerateEmptyChartScript();
            }
        }

        /// <summary>
        /// 주차 계산: .NET 기본 FirstFourDayWeek. ISO-8601 필요 시 별도 로직 사용 가능
        /// </summary>
        private int GetWeekNumber(DateTime date)
        {
            var currentCulture = CultureInfo.CurrentCulture;
            var calendar = currentCulture.Calendar;

            return calendar.GetWeekOfYear(
                date,
                currentCulture.DateTimeFormat.CalendarWeekRule,
                currentCulture.DateTimeFormat.FirstDayOfWeek
            );
        }

        /// <summary>
        /// (WeekNumber, NgLabel)로 그룹화하여 chart.js용 JSON 생성
        /// </summary>
        private string GenerateChartScript(List<VisionNgDTO> visionNgDataWeek)
        {
            // 1) 주차+라벨별로 GroupBy → Count()
            var grouped = visionNgDataWeek
                .GroupBy(d => new { d.WeekNumber, d.NgLabel })
                .Select(g => new
                {
                    WeekNumber = g.Key.WeekNumber,
                    NgLabel = g.Key.NgLabel,
                    LabelCount = g.Count()
                })
                .ToList();

            // 2) X축: 주차 목록
            var distinctWeeks = grouped.Select(x => x.WeekNumber).Distinct().OrderBy(w => w).ToList();
            // 세로축 시리즈: 라벨 목록
            var distinctLabels = grouped.Select(x => x.NgLabel).Distinct().ToList();

            // 3) datasets 구성
            var datasets = new List<object>();
            int colorIndex = 0;

            // 라벨(Mixed, Crack 등)마다 데이터를 구성
            foreach (var lbl in distinctLabels)
            {
                // 현재 라벨에 해당하는 각 주차의 Count
                var dataPoints = new List<int>();
                foreach (var w in distinctWeeks)
                {
                    var matched = grouped.FirstOrDefault(g => g.WeekNumber == w && g.NgLabel == lbl);
                    dataPoints.Add(matched != null ? matched.LabelCount : 0);
                }

                string color = _colorPalette[colorIndex % _colorPalette.Count].Replace("1)", "0.6)");
                colorIndex++;

                datasets.Add(new
                {
                    label = lbl,
                    data = dataPoints,
                    backgroundColor = color
                });
            }

            // 주차를 보기 좋게 "Week X" 로 만들고 싶으면 여기서 문자열로 변환
            var weekLabelsStr = distinctWeeks.Select(w => $"Week {w}").ToList();

            // 4) Chart.js 구성 객체
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

            // 5) 직렬화 → JSON
            return JsonSerializer.Serialize(config);
        }

        /// <summary>
        /// 데이터가 없을 때 표시할 차트
        /// </summary>
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
