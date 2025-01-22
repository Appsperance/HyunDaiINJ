using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Models.Monitoring.Vision;

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class VisionWeekViewModel : INotifyPropertyChanged
    {
        private readonly MSDApi _api;

        private DispatcherTimer _timer; // 폴링용 타이머

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

        public event Action ChartScriptUpdated;
        public event PropertyChangedEventHandler PropertyChanged;

        public VisionWeekViewModel()
        {
            _api = new MSDApi();
            WeekDataList = new List<VisionNgDTO>();

            // (1) 타이머 초기화 (예: 10초 간격)
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _timer.Tick += async (s, e) =>
            {
                await LoadVisionNgDataWeekAsync(); // 주기적으로 데이터 로드
            };
            _timer.Start();
        }

        public async Task LoadVisionNgDataWeekAsync()
        {
            try
            {
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

                // WeekDataList 업데이트
                WeekDataList = dtoList;

                // (3) groupBy
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

                // (5) 이벤트 알림 (View에서 ChartScriptUpdated를 구독 중이면 차트 새로고침)
                OnChartScriptUpdated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadVisionNgDataWeekAsync] error: {ex.Message}");
            }
        }

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

        private string BuildWeekChartScript(IEnumerable<dynamic> grouped)
        {
            var distinctWeeks = grouped
                .Select(x => (int)x.WeekNumber)
                .Distinct()
                .OrderBy(w => w)
                .ToList();

            var distinctLabels = grouped
                .Select(x => (string)x.NgLabel)
                .Distinct()
                .ToList();

            var datasets = new List<object>();
            var colorPalette = new List<string>
            {
                "rgba(75, 192, 192, 0.7)",
                "rgba(255, 99, 132, 0.7)",
                "rgba(54, 162, 235, 0.7)",
                "rgba(255, 206, 86, 0.7)",
                "rgba(153, 102, 255, 0.7)",
                "rgba(255, 159, 64, 0.7)",
                "rgba(201, 203, 207, 0.7)" // 7번째 (연한 회색)
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

            var weekLabels = distinctWeeks.Select(wn => $"{wn} 주차").ToArray();

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
                                color = "#404040"
                            },
                            ticks = new
                            {
                                color = "#95C0FF"
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
                                color = "#404040"
                            },
                            ticks = new
                            {
                                color = "#95C0FF"
                            }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(config);
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private void OnChartScriptUpdated()
            => ChartScriptUpdated?.Invoke();
    }
}
