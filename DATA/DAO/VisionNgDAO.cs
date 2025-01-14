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
                            DateTime = reader["date_str"]?.ToString(),

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
        var visionNgDataList = new List<VisionNgDTO>();
        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = VisionNgQueries.GetVisionNgDataDaily;

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = new VisionNgDTO
                        {
                            // ng_label 컬럼 → NgLabel 프로퍼티
                            NgLabel = reader["ng_label"]?.ToString(),

                            // label_count 컬럼 → LabelCount 프로퍼티 (int)
                            LabelCount = reader.GetInt32(reader.GetOrdinal("label_count"))
                        };

                        visionNgDataList.Add(data);

                        // 로깅 (옵션)
                        Console.WriteLine($"asdfasdfasdfasdfasdf GetVisionNgDataDaily: {data.NgLabel}, Count: {data.LabelCount}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetVisionNgDataDaily: {ex.Message}");
            throw;
        }
        return visionNgDataList;
    }

    public List<VisionNgDTO> GetVisionNgDataYear()
    {
        var visionNgDataList = new List<VisionNgDTO>();
        try
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = VisionNgQueries.GetVisionNgDataYear;
                // 예: "SELECT EXTRACT(YEAR FROM date_time) AS year_num, ng_label, COUNT(*) AS label_count FROM vision_ng GROUP BY year_num, ng_label ..."

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // (1) year_num은 double로 읽은 뒤 (int) 캐스팅
                        double yearDouble = reader.GetDouble(reader.GetOrdinal("year_num"));
                        int yearNum = (int)yearDouble;

                        var data = new VisionNgDTO
                        {
                            YearNumber = (int)reader.GetDouble(reader.GetOrdinal("year_num")),         // EXTRACT(YEAR ...) → YearNumber
                            NgLabel = reader["ng_label"]?.ToString(), // ng_label
                            LabelCount = reader.GetInt32(reader.GetOrdinal("label_count")) // label_count
                        };

                        visionNgDataList.Add(data);

                        // 로깅 (옵션)
                        Console.WriteLine(
                            $"GetVisionNgDataYear: Year={data.YearNumber}, NgLabel={data.NgLabel}, Count={data.LabelCount}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetVisionNgDataYear: {ex.Message}");
            throw;
        }
        return visionNgDataList;
    }


}
