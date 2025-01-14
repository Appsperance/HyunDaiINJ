// VisionWeekViewModel.cs
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Net.Sockets;
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

        public event Action ChartScriptUpdated; // 이벤트 추가
        public event PropertyChangedEventHandler PropertyChanged;

        public string ChartScript
        {
            get => _chartScript;
            private set
            {
                _chartScript = value;
                OnPropertyChanged();
                ChartScriptUpdated?.Invoke(); // 이벤트 호출
            }
        }

        public VisionWeekViewModel()
        {
            _visionNgModel = new VisionNgModel();

            // 색상 팔레트 초기화
            _colorPalette = new List<string>
            {
                "rgba(75, 192, 192, 1)",  // 청록색
                "rgba(255, 99, 132, 1)",  // 빨강
                "rgba(54, 162, 235, 1)",  // 파랑
                "rgba(255, 206, 86, 1)",  // 노랑
                "rgba(153, 102, 255, 1)"  // 보라
            };
        }

        // 서버에서 데이터를 수신하고 UI를 업데이트하는 메서드
        public async Task ListenToServerAsync()
        {
            try
            {
                using var client = new TcpClient("127.0.0.1", 51900);
                using var stream = client.GetStream();
                byte[] buffer = new byte[4096];

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    await LoadVisionNgDataWeekAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" 서버 연결 에러: {ex.Message}");
            }
        }


        // 데이터 가져오기 및 Chart.js 스크립트 생성
        public async Task LoadVisionNgDataWeekAsync()
        {
            try
            {
                var visionNgDataWeek = await Task.Run(() => _visionNgModel.GetVisionNgDataWeek());

                if (visionNgDataWeek == null || visionNgDataWeek.Count == 0)
                {
                    ChartScript = GenerateEmptyChartScript();
                    return;
                }

                ChartScript = GenerateChartScript(visionNgDataWeek);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"주간 Ng_Label 데이터 에러 : {ex.Message}");
            }
        }


        private string GenerateChartScript(List<VisionNgDTO> visionNgDataWeek)
        {
            var datasets = new List<object>();
            var labels = new List<string>();
            var groupedData = new Dictionary<string, Dictionary<int, int>>();

            if (visionNgDataWeek == null || visionNgDataWeek.Count == 0)
            {
                return GenerateEmptyChartScript(); // 빈 차트 스크립트 생성
            }

            try
            {
                // Group data
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
                    if (visionNgDataWeek == null || visionNgDataWeek.Count == 0)
                    {
                        return string.Empty;
                    }

                    groupedData[data.NgLabel][data.WeekNumber] += data.LabelCount;

                    var weekLabel = $"Week {data.WeekNumber}";
                    if (!labels.Contains(weekLabel))
                    {
                        labels.Add(weekLabel);
                    }
                }

                int colorIndex = 0;
                foreach (var group in groupedData)
                {
                    var dataPoints = new List<int>();

                    foreach (var label in labels)
                    {
                        var weekNumber = int.Parse(label.Replace("Week ", ""));
                        var value = group.Value.ContainsKey(weekNumber) ? group.Value[weekNumber] : 0;
                        dataPoints.Add(value);
                    }

                    datasets.Add(new
                    {
                        label = group.Key,
                        data = dataPoints,
                        backgroundColor = _colorPalette[colorIndex % _colorPalette.Count].Replace("1)", "0.6)")
                    });

                    colorIndex++;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error processing data: {ex.Message}");
                throw;
            }

            var config = new
            {
                type = "bar",
                data = new
                {
                    labels = labels,
                    datasets = datasets
                },
                options = new
                {
                    responsive = true,
                    plugins = new
                    {

                    },
                    scales = new
                    {
                        x = new { stacked = true },
                        y = new { stacked = true }
                    }
                }
            };

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
                    labels = new string[] { "No Data" },
                    datasets = new object[]
                    {
                new
                {
                    label = "Empty Dataset",
                    data = new int[] { 0 },
                    backgroundColor = "rgba(200, 200, 200, 0.5)"
                }
                    }
                },
                options = new
                {
                    responsive = true,
                    plugins = new
                    {
                        title = new
                        {
                            display = true,
                            text = "No Data Available"
                        }
                    },
                    scales = new
                    {
                        x = new { stacked = true },
                        y = new { stacked = true }
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