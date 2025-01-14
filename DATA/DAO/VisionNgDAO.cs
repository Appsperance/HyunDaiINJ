using HyunDaiINJ.DATA.DAO;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.DATA.Queries;
using Npgsql;
using System.Collections.Generic;
using System;
using WpfApp1;

public class VisionNgDAO : IVisionNgDAO
{
    private readonly string _connectionString;

    public VisionNgDAO()
    {
        _connectionString = App.ConnectionString
            ?? throw new InvalidOperationException("ConnectionString has not been initialized.");
    }

    public List<VisionNgDTO> GetVisionNgData()
    {
        var visionNgData = new List<VisionNgDTO>();
        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = VisionNgQueries.GetVisionNgData;

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        visionNgData.Add(new VisionNgDTO
                        {
                            NgLabel = reader["ng_label"]?.ToString(),
                            LabelCount = reader.GetInt32(reader.GetOrdinal("label_count"))
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetVisionNgData: {ex.Message}");
            throw;
        }
        return visionNgData;
    }

    public List<VisionNgDTO> GetVisionNgDataAll()
    {
        var visionNgDataAll = new List<VisionNgDTO>();
        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = VisionNgQueries.GetVisionNgDataAll;

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        visionNgDataAll.Add(new VisionNgDTO
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
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetVisionNgDataAll: {ex.Message}");
            throw;
        }
        return visionNgDataAll;
    }

    public List<VisionNgDTO> GetVisionNgDataWeek()
    {
        var visionNgDataWeek = new List<VisionNgDTO>();
        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = VisionNgQueries.GetVisionNgDataWeek;

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = new VisionNgDTO
                        {
                            YearNumber = reader.GetInt32(reader.GetOrdinal("year_number")),
                            WeekNumber = reader.GetInt32(reader.GetOrdinal("week_number")),
                            WeekStartDate = reader.GetDateTime(reader.GetOrdinal("week_start_date")),
                            WeekEndDate = reader.GetDateTime(reader.GetOrdinal("week_end_date")),
                            NgLabel = reader["ng_label"]?.ToString(),
                            LabelCount = reader.GetInt32(reader.GetOrdinal("ng_count"))
                        };

                        visionNgDataWeek.Add(data);

                        // 각 데이터 출력
                        Console.WriteLine($"Year: {data.YearNumber}, Week: {data.WeekNumber}, Label: {data.NgLabel}, Count: {data.LabelCount}");
                    }

                    Console.WriteLine($"visiongNG : ", visionNgDataWeek.Count);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetVisionNgDataAll: {ex.Message}");
            throw;
        }
        return visionNgDataWeek;
    }

    public List<VisionNgDTO> GetVisionNgDataDaily()
    {
        var visionNgDataWeek = new List<VisionNgDTO>();
        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = VisionNgQueries.GetVisionNgDataWeek;

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = new VisionNgDTO
                        {
                            YearNumber = reader.GetInt32(reader.GetOrdinal("year_number")),
                            WeekNumber = reader.GetInt32(reader.GetOrdinal("week_number")),
                            WeekStartDate = reader.GetDateTime(reader.GetOrdinal("week_start_date")),
                            WeekEndDate = reader.GetDateTime(reader.GetOrdinal("week_end_date")),
                            NgLabel = reader["ng_label"]?.ToString(),
                            LabelCount = reader.GetInt32(reader.GetOrdinal("ng_count"))
                        };

                        visionNgDataWeek.Add(data);

                        // 각 데이터 출력
                        Console.WriteLine($"Year: {data.YearNumber}, Week: {data.WeekNumber}, Label: {data.NgLabel}, Count: {data.LabelCount}");
                    }

                    Console.WriteLine($"visiongNG : ", visionNgDataWeek.Count);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetVisionNgDataAll: {ex.Message}");
            throw;
        }
        return visionNgDataWeek;
    }

}
