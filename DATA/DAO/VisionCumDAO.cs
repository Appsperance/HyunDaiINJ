using System;
using System.Collections.Generic;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.DATA.Queries;
using Npgsql;
using WpfApp1;

namespace HyunDaiINJ.DATA.DAO
{
    public class VisionCumDAO : IVisionCumDAO
    {
        private readonly string _connectionString;

        public VisionCumDAO()
        {
            // App.ConnectionString이 초기화되지 않은 경우 예외 발생
            _connectionString = App.ConnectionString
                ?? throw new InvalidOperationException("ConnectionString has not been initialized.");
        }

        public List<VisionCumDTO> GetVisionCumData()
        {
            var visionDataList = new List<VisionCumDTO>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Database connection opened successfully.");

                    // 데이터 조회 쿼리
                    string query = VisionCumQueries.GetVisionData;

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {   
                                    visionDataList.Add(new VisionCumDTO
                                    {
                                        lineId = reader["line_id"]?.ToString(),
                                        time = reader.GetDateTime(reader.GetOrdinal("time")),
                                        lotId = reader["lot_id"]?.ToString(),
                                        shift = reader["shift"]?.ToString(),
                                        employeeNumber = reader.GetInt64(reader.GetOrdinal("employee_number")),
                                        total = reader.GetInt32(reader.GetOrdinal("total"))
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
                Console.WriteLine($"Retrieved {visionDataList.Count} records from the database.");
            }

            return visionDataList;
        }
    }
}
