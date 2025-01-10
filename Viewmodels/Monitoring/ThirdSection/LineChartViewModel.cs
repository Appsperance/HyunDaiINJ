using System;
using System.Collections.Generic;
using System.Text.Json;
using HyunDaiINJ.DATA.DTO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

            // ChartScript 기본값 설정
            ChartScript = this.GenerateEmptyChartScript();
            Console.WriteLine($"[DEBUG] Initial ChartScript: {ChartScript}");

        }


        public async Task ConnectToSocketServerAsync()
        {
            try
            {
                using var client = new TcpClient("127.0.0.1", 51900);
                using var stream = client.GetStream();
                byte[] buffer = new byte[8192]; // 버퍼 크기를 충분히 크게 설정
                StringBuilder completeData = new StringBuilder(); // 데이터를 누적할 StringBuilder

                string request = "get_data";
                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // 연결이 종료된 경우

                    // 받은 데이터를 문자열로 변환하고 누적
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    completeData.Append(receivedData);

                    // JSON이 완전한 형식인지 확인
                    if (IsValidJson(completeData.ToString()))
                    {
                        var jsonData = completeData.ToString();
                        completeData.Clear(); // 데이터를 초기화

                        Console.WriteLine($"[DEBUG] Full JSON Data Received: {jsonData}");
                        var data = JsonSerializer.Deserialize<List<VisionCumDTO>>(jsonData);

                        if (data != null && data.Count > 0)
                        {
                            ChartScript = GenerateChartScript(data); // 데이터를 기반으로 ChartScript 생성
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Socket error: {ex.Message}");
            }
        }

        // JSON 유효성 검사 메서드 추가
        private bool IsValidJson(string jsonData)
        {
            try
            {
                JsonDocument.Parse(jsonData);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public string GenerateEmptyChartScript()
        {
            var config = new
            {
                type = "line",
                data = new
                {
                    labels = new string[] { "No Data" },
                    datasets = new object[]
                    {
                new
                {
                    label = "Empty Dataset",
                    data = new int[] { 0 },
                    borderColor = "rgba(200, 200, 200, 1)",
                    fill = false
                }
                    }
                },
                options = new
                {
                    responsive = true,
                    scales = new
                    {
                        x = new { type = "category" },
                        y = new { beginAtZero = true }
                    }
                }
            };

            return JsonSerializer.Serialize(config);
        }


        private string GenerateChartScript(List<VisionCumDTO> visionCumData)
        {
            var datasets = new List<object>();
            var labels = new List<string>();
            var dataPoints = new List<int>();

            foreach (var entry in visionCumData)
            {
                // time과 total만 사용
                labels.Add(entry.lotId); // X축 라벨
                dataPoints.Add(entry.total ?? 0); // Y축 데이터
            }

            datasets.Add(new
            {
                label = "Total per Day",
                data = dataPoints,
                borderColor = "rgba(75, 192, 192, 1)",
                fill = false,
                tension = 0.4
            });

            var config = new
            {
                type = "line",
                data = new
                {
                    labels = labels,
                    datasets = datasets
                },
                options = new
                {
                    responsive = true,
                    scales = new
                    {
                        x = new { type = "category" },
                        y = new { beginAtZero = true }
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
