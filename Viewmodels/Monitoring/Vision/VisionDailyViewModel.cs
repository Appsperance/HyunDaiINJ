using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;
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

        // 뷰에게 "차트 스크립트가 갱신됐다"는 이벤트 알림
        public event Action ChartScriptUpdated;

        public VisionDailyViewModel()
        {
            _api = new MSDApi();
            DailyDataList = new List<VisionNgDTO>();
        }

        /// <summary>
        /// 예시: 2025-01-16 0시(KST) ~ 1/17 0시(KST) 범위 필터 후 NgLabel별 count → 차트 스크립트 생성
        /// </summary>
        public async Task LoadVisionNgDataDailyAsync()
        {
            try
            {
                // (1) API 호출
                var lineIds = new List<string> { "vp01", "vi01", "vp03", "vp04", "vp05" };
                int offset = 0;
                int count = 500;

                var allData = await _api.GetNgImagesAsync(lineIds, offset, count);
               
                // (2) 날짜 범위 설정 (오늘 0시 ~ 내일 0시)
                var targetDateKst = DateTime.Today; // 오늘 0시, PC가 KST라고 가정
                var nextDayKst = targetDateKst.AddDays(1);

                // (3) 필터
                var filteredData = allData.Where(d =>
                {
                    if (DateTimeOffset.TryParse(d.DateTime, out var dto))
                    {
                        var kstTime = dto.LocalDateTime; // PC가 KST라고 가정
                        return (kstTime >= targetDateKst && kstTime < nextDayKst);
                    }
                    return false;
                }).ToList();

                if (filteredData.Count == 0)
                {
                    DailyDataList.Clear();
                    return;
                }

                // (4) NgLabel별 집계 → LabelCount
                var grouped = filteredData
                    .GroupBy(d => d.NgLabel)
                    .Select(g => new { NgLabel = g.Key, Count = g.Count() })
                    .ToList();

                // (5) 가장 이른 KST시각
                var earliestKst = filteredData
                    .Select(d => DateTimeOffset.Parse(d.DateTime).LocalDateTime)
                    .Min();

                // (6) 차트 스크립트 생성
                string configJson = BuildDailyChartScript(grouped, earliestKst);
                ChartScript = configJson;

                // (7) DailyDataList 바인딩(필요 시)
                DailyDataList = filteredData;

                // (8) 알림 이벤트
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
                "rgba(255, 159, 64, 0.7)"
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
                        y = new
                        {
                            beginAtZero = true,
                            title = new
                            {
                                display = true,
                                text = "불량수"
                            }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(config);
        }

        // INotifyPropertyChanged 구현
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private void OnChartScriptUpdated()
        {
            ChartScriptUpdated?.Invoke();
        }
        #endregion
    }
}
