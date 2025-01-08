using System;
using System.Collections.Generic;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.DATA.Queries;
using Npgsql;
using WpfApp1;

namespace HyunDaiINJ.DATA.DAO
{
    public class VisionNgDAO : IVisionNgDAO
    {
        private readonly string _connectionString;

        public VisionNgDAO()
        {
            // App.ConnectionString이 초기화되지 않은 경우 예외 발생
            _connectionString = App.ConnectionString
                ?? throw new InvalidOperationException("ConnectionString has not been initialized.");
        }

        public List<VisionNgDTO> GetVisionNgData() // 인터페이스 멤버 구현
        {
            var visionNgData = new List<VisionNgDTO>();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Database connection opened successfully.");

                    // 데이터 조회 쿼리
                    string query = VisionNgQueries.GetVisionNgData;

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    visionNgData.Add(new VisionNgDTO
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                                        LotId = reader["lot_id"]?.ToString(),
                                        PartId = reader["part_id"]?.ToString(),
                                        LineId = reader["line_id"]?.ToString(),
                                        DateTime = reader.GetDateTime(reader.GetOrdinal("date_time")),
                                        NgLabel = reader["ng_label"]?.ToString(),
                                        NgImgPath = reader["ng_img_path"]?.ToString()
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Data parsing error: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException npgsqlEx)
            {
                Console.WriteLine($"Database error: {npgsqlEx.Message}");
                throw; // 필요하면 예외를 상위로 전달
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw; // 필요하면 예외를 상위로 전달
            }
            finally
            {
                Console.WriteLine($"Retrieved {visionNgData.Count} records from the database.");
            }
            return visionNgData;
        }
    }
}
