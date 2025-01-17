using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using HyunDaiINJ.DATA.DTO;  // VisionNgDTO 정의
using HyunDaiINJ.Models.Monitoring.Vision; // MSDApi 등

namespace HyunDaiINJ.ViewModels.Monitoring.vision
{
    public class VisionDailyViewModel : INotifyPropertyChanged
    {
        private readonly MSDApi _api;

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

        public event Action ChartScriptUpdated;

        public VisionDailyViewModel()
        {
            _api = new MSDApi();
            DailyDataList = new List<VisionNgDTO>();
        }

        /// <summary>
        /// 예) 2025-01-16 0시(KST)~17일 0시(KST) 범위를 필터한 뒤, 
        ///     NgLabel별 행 개수를 구하고 차트 스크립트를 만든다.
        /// </summary>
        public async Task LoadVisionNgDataDailyAsync()
        {
            try
            {
                // 1) API 통해 DB 데이터 조회
                var lineIds = new List<string> { "vp01", "vp02", "vp03", "vp04", "vp05" };
                int offset = 0;
                int count = 500;

                var allData = await _api.GetNgImagesAsync(lineIds, offset, count);
                if (allData == null || allData.Count == 0)
                {
                    Console.WriteLine("[LoadVisionNgDataDailyAsync] No data returned from API");
                    return;
                }

                Console.WriteLine("[LoadVisionNgDataDailyAsync] Raw items from API (UTC-based):");
                foreach (var d in allData)
                {
                    if (DateTimeOffset.TryParse(d.DateTime, out var dto))
                    {
                        Console.WriteLine($"  ID={d.Id}, UTC={dto.UtcDateTime}, KST={dto.LocalDateTime}, NgLabel={d.NgLabel}");
                    }
                    else
                    {
                        Console.WriteLine($"  ID={d.Id}, invalid datetime={d.DateTime}");
                    }
                }

                // 2) 예) 1/16 0시(KST) ~ 1/17 0시(KST) 범위
                var targetDateKst = new DateTime(2025, 1, 16, 0, 0, 0, DateTimeKind.Local);
                var nextDayKst = targetDateKst.AddDays(1);
                Console.WriteLine($"[LoadVisionNgDataDailyAsync] Filtering {targetDateKst} ~ {nextDayKst} (KST)");

                // 3) 필터: KST 시각 >= targetDateKst && < nextDayKst
                var filteredData = allData.Where(d =>
                {
                    if (DateTimeOffset.TryParse(d.DateTime, out var dto))
                    {
                        var kstTime = dto.LocalDateTime; // PC가 KST 가정
                        return (kstTime >= targetDateKst && kstTime < nextDayKst);
                    }
                    return false;
                }).ToList();

                if (filteredData.Count == 0)
                {
                    Console.WriteLine("[LoadVisionNgDataDailyAsync] No data found in that KST range");
                    return;
                }

                // 4) NgLabel 기준 "행 개수" 집계 => LabelCount
                var grouped = filteredData
                    .GroupBy(d => d.NgLabel)
                    .Select(g => new VisionNgDTO
                    {
                        NgLabel = g.Key,
                        LabelCount = g.Count()
                    })
                    .ToList();

                // 5) X축 라벨용: 필터된 데이터 중 가장 이른 KST 시각
                var earliestKst = filteredData
                    .Select(d => DateTimeOffset.Parse(d.DateTime).LocalDateTime)
                    .Min(); // DateTime (KST)

                // 6) 차트 구성
                var chartConfig = CreateChartConfig(grouped, earliestKst);
                string configJson = JsonSerializer.Serialize(chartConfig);
                ChartScript = configJson;

                // 7) 바인딩/차트 업데이트
                OnChartScriptUpdated();

                Console.WriteLine("[LoadVisionNgDataDailyAsync] ChartScript updated with UTC↔KST logic");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadVisionNgDataDailyAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// CreateChartConfig - X축 라벨=earliestKst, dataset=NgLabel별 LabelCount
        /// </summary>
        private object CreateChartConfig(List<VisionNgDTO> groupedData, DateTime earliestKst)
        {
            var xLabel = earliestKst.ToString("yyyy-MM-dd");
            var xLabels = new[] { xLabel };

            // NgLabel, LabelCount 추출
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
            foreach (var item in groupedData)
            {
                datasets.Add(new
                {
                    label = item.NgLabel,
                    data = new[] { item.LabelCount },
                    backgroundColor = colorPalette[colorIndex % colorPalette.Count]
                });
                colorIndex++;
            }

            // Chart.js config
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

            return config;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private void OnChartScriptUpdated() => ChartScriptUpdated?.Invoke();
        #endregion
    }
}
