using System;
using System.Collections.Generic;
using System.Text.Json;
using HyunDaiINJ.Models.Monitoring.ThirdSection;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.ViewModels.Monitoring.ThirdSection
{
    public class LineChartViewModel
    {
        private readonly VisionCumModel _visionCumModel;
        private readonly List<string> _colorPalette; // 미리 정의된 색상 팔레트

        public LineChartViewModel()
        {
            try
            {
                // VisionCumModel 초기화
                _visionCumModel = new VisionCumModel();
                Console.WriteLine("LineChartViewModel: VisionCumModel initialized successfully.");

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
            catch (Exception ex)
            {
                Console.WriteLine($"LineChartViewModel: Error initializing VisionCumModel - {ex.Message}");
                throw;
            }
        }

        public string GenerateChartScript()
        {
            List<VisionCumDTO> visionData;
            try
            {
                // VisionCumModel에서 데이터 가져오기
                visionData = _visionCumModel.GetVisionCumData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LineChartViewModel: Error retrieving data from VisionCumModel - {ex.Message}");
                throw;
            }

            // 데이터 변환: VisionCumDTO를 Chart.js 데이터 형식으로 변환
            var datasets = new List<object>();
            var labels = new List<string>();
            var groupedData = new Dictionary<string, List<int>>();

            try
            {
                int colorIndex = 0;

                foreach (var data in visionData)
                {
                    // lot_id 기준으로 데이터를 그룹화
                    if (!groupedData.ContainsKey(data.lotId))
                    {
                        groupedData[data.lotId] = new List<int>();
                    }

                    groupedData[data.lotId].Add((int)data.total);

                    // 날짜 레이블 추가
                    var dateLabel = data.time.ToString("MM/dd");
                    if (!labels.Contains(dateLabel))
                    {
                        labels.Add(dateLabel);
                    }
                }

                // Chart.js용 데이터셋 생성
                foreach (var group in groupedData)
                {
                    var color = _colorPalette[colorIndex % _colorPalette.Count]; // 순환 색상 할당
                    var backgroundColor = color.Replace("1)", "0.2)"); // 배경색은 투명도 적용

                    datasets.Add(new
                    {
                        label = group.Key, // lot_id
                        data = group.Value, // 총계(total) 값
                        borderColor = color,
                        backgroundColor = backgroundColor,
                        fill = true,
                        tension = 0.4
                    });

                    colorIndex++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            // Chart.js 구성 데이터 생성
            var config = new
            {
                type = "line",
                data = new
                {
                    labels = labels, // X축 레이블
                    datasets = datasets // 데이터셋
                },
                options = new
                {
                    plugins = new
                    {
                        legend = new
                        {
                            display = true,
                            position = "top"
                        }
                    },
                    scales = new
                    {
                        x = new
                        {
                            type = "category",
                            ticks = new 
                            {
                                autoSkip = false, // 축 레이블 자동 생략 방지
                            },

                            grid = new
                            {
                                display = false
                            }
                        },
                        y = new
                        {
                            ticks = new
                            {
                                stepSize = 50
                            },
                            grid = new
                            {
                                color = "rgba(200, 200, 200, 0.2)"
                            }
                        }
                    }
                }
            };

            // JSON 형식으로 반환
            try
            {
                var script = $@"
                    const config = {JsonSerializer.Serialize(config)};
                    const ctx = document.getElementById('chart1').getContext('2d');
                    new Chart(ctx, config);
                ";
                return script;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
