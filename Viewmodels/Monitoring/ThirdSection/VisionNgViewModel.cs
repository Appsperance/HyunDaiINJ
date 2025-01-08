using System;
using System.Collections.Generic;
using System.Text.Json;
using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.ViewModels.Monitoring.ThirdSection
{
    public class VisionNgViewModel
    {
        private readonly VisionNgDAO _visionNgDAO;

        public VisionNgViewModel()
        {
            _visionNgDAO = new VisionNgDAO();
        }

        public string GenerateChartScript()
        {
            // DAO에서 데이터 가져오기
            List<VisionNgDTO> chartData = _visionNgDAO.GetVisionNgData();

            // Chart.js 데이터 변환
            var labels = new List<string>();
            var data = new List<int>();

            foreach (var record in chartData)
            {
                labels.Add(record.NgLabel);
                data.Add(record.LabelCount);
            }

            // Chart.js 구성 JSON 생성
            var config = new
            {
                type = "pie",
                data = new
                {
                    labels = labels,
                    datasets = new[]
                    {
                        new
                        {
                            data = data,
                            backgroundColor = new[]
                            {
                                "rgba(255, 99, 132, 0.2)", // Red
                                "rgba(54, 162, 235, 0.2)", // Blue
                                "rgba(255, 206, 86, 0.2)", // Yellow
                                "rgba(75, 192, 192, 0.2)", // Green
                                "rgba(153, 102, 255, 0.2)", // Purple
                                "rgba(255, 159, 64, 0.2)"   // Orange
                            },
                            borderColor = new[]
                            {
                                "rgba(255, 99, 132, 1)",
                                "rgba(54, 162, 235, 1)",
                                "rgba(255, 206, 86, 1)",
                                "rgba(75, 192, 192, 1)",
                                "rgba(153, 102, 255, 1)",
                                "rgba(255, 159, 64, 1)"
                            },
                            borderWidth = 1
                        }
                    }
                },
                options = new
                {
                    plugins = new
                    {
                        legend = new
                        {
                            display = true,
                            position = "right"
                        }
                    }
                }
            };

            // Chart.js 스크립트 생성
            return $@"
                const config = {JsonSerializer.Serialize(config)};
                const ctx = document.getElementById('chart1').getContext('2d');
                new Chart(ctx, config);
            ";
        }
    }
}
