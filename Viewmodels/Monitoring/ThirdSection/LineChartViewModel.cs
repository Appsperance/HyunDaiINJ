using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using Newtonsoft.Json;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.ViewModels.Monitoring.ThirdSection
{
    public class LineChartViewModel : INotifyPropertyChanged
    {
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
                ChartScriptUpdated?.Invoke();
            }
        }

        public LineChartViewModel()
        {
            _colorPalette = new List<string>
            {
                "rgba(75, 192, 192, 1)",
                "rgba(255, 99, 132, 1)",
                "rgba(54, 162, 235, 1)",
                "rgba(255, 206, 86, 1)",
                "rgba(153, 102, 255, 1)"
            };

            ChartScript = GenerateEmptyChartScript();
        }

        public async Task ConnectToSocketServerAsync()
        {
            try
            {
                using var client = new TcpClient("127.0.0.1", 51900);
                using var stream = client.GetStream();
                var buffer = new byte[8192];
                var completeData = new StringBuilder();

                byte[] requestBytes = Encoding.UTF8.GetBytes("get_data");
                await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                Console.WriteLine($"[DEBUG] Sent request to server: {Encoding.UTF8.GetString(requestBytes)}");

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("[DEBUG] Connection closed by server.");
                        break;
                    }

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[DEBUG] Received data chunk: {receivedData}");

                    completeData.Append(receivedData);

                        var jsonData = completeData.ToString();
                        Console.WriteLine($"[DEBUG] Complete JSON Data: {jsonData}");

                        completeData.Clear(); // Clear after processing JSON

                        try
                        {
                            var data = JsonConvert.DeserializeObject<List<VisionCumDTO>>(jsonData);
                            if (data != null && data.Any())
                            {
                                Console.WriteLine("[DEBUG] Deserialized Data:");
                                foreach (var entry in data)
                                {
                                    Console.WriteLine($"- Time: {entry.time}, LotId: {entry.lotId}, Total: {entry.total}");
                                }

                                ChartScript = GenerateChartScript(data);
                            }
                            else
                            {
                                Console.WriteLine("[DEBUG] No valid data found in JSON.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Deserialization failed: {ex.Message}");
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Socket error: {ex.Message}");
            }
        }

        public string GenerateEmptyChartScript()
        {
            var config = new
            {
                type = "line",
                data = new { labels = new[] { "No Data" }, datasets = new[] { new { label = "Empty", data = new[] { 0 } } } },
                options = new { responsive = true, scales = new { x = new { type = "time" }, y = new { beginAtZero = true } } }
            };

            return JsonConvert.SerializeObject(config);
        }

        private string GenerateChartScript(List<VisionCumDTO> visionCumData)
        {
            Console.WriteLine($"[DEBUG] Input Data Count: {visionCumData.Count}");

            // 1. 유효한 데이터 필터링
            var validData = visionCumData.Where(entry => entry.total > 0).ToList();
            Console.WriteLine($"[DEBUG] Valid Data Count: {validData.Count}");
            foreach (var entry in validData)
            {
                Console.WriteLine($"- Time: {entry.time}, LotId: {entry.lotId}, Total: {entry.total}");
            }

            // 2. 라벨 생성 (lotId 기준)
            var labels = validData.Select(e => e.lotId)
                                  .Distinct()
                                  .OrderBy(e => e)
                                  .ToList();
            Console.WriteLine($"[DEBUG] Labels (LotId): {string.Join(", ", labels)}");

            // 3. X축 데이터를 유니크한 시간(time) 값으로 생성
            var uniqueTimes = validData.Select(e => e.time.ToString("yyyy-MM-dd HH:mm"))
                                        .Distinct()
                                        .OrderBy(e => e)
                                        .ToList();
            Console.WriteLine($"[DEBUG] Unique Times (X-axis): {string.Join(", ", uniqueTimes)}");

            // 4. 데이터셋 생성 (lotId별 데이터)
            var datasets = validData
                .GroupBy(e => e.lotId)
                .Select((group, index) =>
                {
                    // X축(time)에 대응하는 Y축(total) 값 생성
                    var dataPoints = uniqueTimes.Select(time =>
                    {
                        var matchingEntry = group.FirstOrDefault(e => e.time.ToString("yyyy-MM-dd HH:mm") == time);
                        return new
                        {
                            x = time,
                            y = matchingEntry?.total ?? 0 // 매칭되는 값이 없으면 0으로 설정
                        };
                    }).ToList();

                    var dataset = new
                    {
                        label = group.Key, // lotId를 데이터셋 라벨로 사용
                        data = dataPoints,
                        borderColor = _colorPalette[index % _colorPalette.Count],
                        fill = false
                    };

                    Console.WriteLine($"[DEBUG] Dataset for LotId {group.Key}:");
                    foreach (var dataPoint in dataset.data)
                    {
                        Console.WriteLine($"  - X: {dataPoint.x}, Y: {dataPoint.y}");
                    }

                    return dataset;
                })
                .ToList();

            Console.WriteLine($"[DEBUG] Datasets Created: {datasets.Count}");

            // 5. Chart.js 구성 생성
            var config = new
            {
                type = "line",
                data = new
                {
                    labels = uniqueTimes, // X축은 time 값으로 설정
                    datasets = datasets
                },
                options = new
                {
                    responsive = true,
                    scales = new
                    {
                        x = new
                        {
                            type = "time", // X축을 time으로 설정
                            time = new
                            {
                                parser = "yyyy-MM-dd HH:mm", // 시간 형식 파싱
                                tooltipFormat = "MMM dd, yyyy HH:mm", // 툴팁 형식
                                displayFormats = new
                                {
                                    hour = "MMM dd HH:mm", // 축 시간 형식
                                    day = "MMM dd"
                                }
                            }
                        },
                        y = new { beginAtZero = true } // Y축은 0부터 시작
                    }
                }
            };

            Console.WriteLine($"[DEBUG] Chart Configuration: {JsonConvert.SerializeObject(config)}");

            return JsonConvert.SerializeObject(config);
        }



        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
