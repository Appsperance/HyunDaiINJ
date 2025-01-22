using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Models.Monitoring.Vision; // MSDApi 등

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class VisionDailyViewModel : INotifyPropertyChanged
    {
        private readonly MSDApi _api;

        // 폴링(10초)용 타이머
        private DispatcherTimer _timer;

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

        // 뷰에게 "차트 스크립트가 갱신됐다"는 이벤트 알림
        public event Action ChartScriptUpdated;

        public event PropertyChangedEventHandler PropertyChanged;

        public VisionDailyViewModel()
        {
            _api = new MSDApi();
            DailyDataList = new List<VisionNgDTO>();

            // (1) 폴링 타이머 설정 (10초마다 재호출)
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _timer.Tick += async (s, e) =>
            {
                Console.WriteLine("[VisionDailyViewModel] Timer tick - calling LoadVisionNgDataDailyAsync()");
                await LoadVisionNgDataDailyAsync();
            };
            _timer.Start();
        }

        /// <summary>
        /// 예시: 2025-01-16 0시(KST) ~ 1/17 0시(KST) 범위 필터 후 NgLabel별 count → 차트 스크립트 생성
        /// </summary>
        public async Task LoadVisionNgDataDailyAsync()
        {
            try
            {
                var lineIds = new List<string> { "vp01", "vi01", "vp03", "vp04", "vp05" };
                int offset = 0;
                int count = 500;

                var allData = await _api.GetNgImagesAsync(lineIds, offset, count);

                // (2) 날짜 범위 설정 (오늘 0시 ~ 내일 0시)
                var targetDateKst = DateTime.Today;
                var nextDayKst = targetDateKst.AddDays(1);

                // (3) 필터
                var filteredData = allData.Where(d =>
                {
                    if (DateTimeOffset.TryParse(d.DateTime, out var dto))
                    {
                        var kstTime = dto.LocalDateTime;
                        return (kstTime >= targetDateKst && kstTime < nextDayKst);
                    }
                    return false;
                }).ToList();

                // (4) 결과 처리
                if (filteredData.Count == 0)
                {
                    DailyDataList.Clear();
                    ChartScript = "";
                    OnChartScriptUpdated();
                    return;
                }

                // (5) NgLabel별 집계 → LabelCount
                var grouped = filteredData
                    .GroupBy(d => d.NgLabel)
                    .Select(g => new { NgLabel = g.Key, Count = g.Count() })
                    .ToList();

                // (6) 가장 이른 시각
                var earliestKst = filteredData
                    .Select(d => DateTimeOffset.Parse(d.DateTime).LocalDateTime)
                    .Min();

                // (7) 차트 스크립트 생성
                string configJson = BuildDailyChartScript(grouped, earliestKst);
                ChartScript = configJson;

                // (8) DailyDataList 갱신
                DailyDataList = filteredData;

                // (9) 알림 이벤트
                OnChartScriptUpdated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadVisionNgDataDailyAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// (내부) 일간 차트를 위한 Chart.js config JSON 생성
        /// </summary>
        private string BuildDailyChartScript(IEnumerable<dynamic> grouped, DateTime earliestKst)
        {
            var xLabel = earliestKst.ToString("yyyy-MM-dd");
            var xLabels = new[] { xLabel };

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
            foreach (var item in grouped)
            {
                datasets.Add(new
                {
                    label = item.NgLabel,
                    data = new[] { item.Count },
                    backgroundColor = colorPalette[colorIndex % colorPalette.Count]
                });
                colorIndex++;
            }

            var config = new
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
                            text = $"일간 불량 ({xLabel}, NgLabel count)"
                        }
                    },
                    scales = new
                    {
                        x = new
                        {
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
                            beginAtZero = true,
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

        private void OnChartScriptUpdated()
        {
            ChartScriptUpdated?.Invoke();
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        #endregion
    }
}
