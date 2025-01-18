using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization; // 주차 계산 시 사용
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Models.Monitoring.Vision; // MSDApi 등

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class VisionWeekViewModel : INotifyPropertyChanged
    {
        private readonly MSDApi _api;

        // 주간 원본 데이터 리스트 (필요 시 UI 표시용)
        private List<VisionNgDTO> _weekDataList;
        public List<VisionNgDTO> WeekDataList
        {
            get => _weekDataList;
            set
            {
                _weekDataList = value;
                OnPropertyChanged();
            }
        }

        // 차트 스크립트
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

        // 차트 갱신 이벤트 (View 측에서 ChartScriptUpdated를 구독)
        public event Action ChartScriptUpdated;

        public event PropertyChangedEventHandler PropertyChanged;

        public VisionWeekViewModel()
        {
            _api = new MSDApi();
            WeekDataList = new List<VisionNgDTO>();
        }

        /// <summary>
        /// 주간 데이터 로드 (API 호출 → WeekNumber 계산 → GroupBy → ChartScript 생성)
        /// </summary>
        public async Task LoadVisionNgDataWeekAsync()
        {
            try
            {
                // (1) API 호출
                var lineIds = new List<string> { "vp01", "vi01", "vp03", "vp04", "vp05" };
                int offset = 0;
                int count = 500;

                var dtoList = await _api.GetNgImagesAsync(lineIds, offset, count);
                if (dtoList == null || dtoList.Count == 0)
                {
                    WeekDataList.Clear();
                    return;
                }


                // (2) 주차 계산
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

                // WeekDataList에 저장 (UI에서 보여줄 수 있음)
                WeekDataList = dtoList;

                // (3) GroupBy(week+label) → count
                var grouped = dtoList
                    .GroupBy(x => new { x.WeekNumber, x.NgLabel })
                    .Select(g => new
                    {
                        WeekNumber = g.Key.WeekNumber,
                        NgLabel = g.Key.NgLabel,
                        LabelCount = g.Count()
                    })
                    .ToList();

                // (4) 차트 스크립트 생성
                var configJson = BuildWeekChartScript(grouped);
                ChartScript = configJson;

                // (5) 이벤트 알림
                OnChartScriptUpdated();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadVisionNgDataWeekAsync] error: {ex.Message}");
            }
        }

        /// <summary>
        /// 주차 계산 (기본 .NET 로직, Culture 따라 다를 수 있음)
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
        /// 차트 JS 스크립트 생성 (주차 * 라벨 → stacked bar)
        /// </summary>
        private string BuildWeekChartScript(IEnumerable<dynamic> grouped)
        {
            // 1) 주차 목록
            var distinctWeeks = grouped
                .Select(x => (int)x.WeekNumber)
                .Distinct()
                .OrderBy(w => w)
                .ToList();

            // 2) 라벨 목록(Mixed, Crack 등)
            var distinctLabels = grouped
                .Select(x => (string)x.NgLabel)
                .Distinct()
                .ToList();

            // 3) datasets
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
            foreach (var lbl in distinctLabels)
            {
                var dataPoints = new List<int>();
                foreach (var w in distinctWeeks)
                {
                    var row = grouped.FirstOrDefault(g => g.WeekNumber == w && g.NgLabel == lbl);
                    dataPoints.Add(row != null ? (int)row.LabelCount : 0);
                }

                var color = colorPalette[colorIndex % colorPalette.Count];
                colorIndex++;

                datasets.Add(new
                {
                    label = lbl,
                    data = dataPoints,
                    backgroundColor = color
                });
            }

            // 4) 주차 표시: "Week 3" 이런 식으로 변환
            var weekLabels = distinctWeeks.Select(wn => $"{wn} 주차").ToArray();

            // 5) chart.js config
            var config = new
            {
                type = "bar",
                data = new
                {
                    labels = weekLabels,
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
                            text = "주차별 불량 현황"
                        }
                    },
                    scales = new
                    {
                        x = new
                        {
                            stacked = false,
                            grid = new
                            {
                                color = "#404040"  // x축 그리드 선 색깔을 #404040으로 설정
                            },
                            ticks = new
                            {
                                color = "#95C0FF"  // x축 tick 색상 설정
                            }
                        },
                        y = new
                        {
                            stacked = false,
                            title = new
                            {
                                display = true,
                                text = "불량수"
                            },
                            grid = new
                            {
                                color = "#404040"  // y축 그리드 선 색깔을 #404040으로 설정
                            },
                            ticks = new
                            {
                                color = "#95C0FF"  // x축 tick 색상 설정
                            }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(config);
        }

        // INotifyPropertyChanged 구현
        #region INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void OnChartScriptUpdated()
        {
            ChartScriptUpdated?.Invoke();
        }

        #endregion
    }
}
