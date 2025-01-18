using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using WpfApp1;    // App.ConnectionString 위치
using HyunDaiINJ.DATA.Queries;
using HyunDaiINJ.DATA.DTO;    // DTO 참조
using System.Text;

namespace HyunDaiINJ.DATA.DAO
{
    public class InjectionPlanDAO
    {
        private readonly string _connectionString;

        /// <summary>
        /// DailyPlanViewModel에서 qtyWeekly 없이 qtyDaily만 받아서 한 번에 INSERT.
        /// (part_id, date, qty_daily, iso_week, day)
        /// </summary>
        public async Task InsertDailyPlansAtOnceAsync(
            List<(string partId, DateTime dateVal, int isoWeek, int qtyDaily, string dayVal)> dataList
        )
        {
            if (dataList == null || dataList.Count == 0)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("INSERT INTO injection_plan (part_id, date, qty_daily, iso_week, day) VALUES");

            var paramList = new List<NpgsqlParameter>();

            for (int i = 0; i < dataList.Count; i++)
            {
                var (partId, dateVal, isoWeek, qtyDaily, dayVal) = dataList[i];

                string pPartId = $"@p_partId_{i}";
                string pDate = $"@p_date_{i}";
                string pIsoWeek = $"@p_isoWeek_{i}";
                string pQtyDaily = $"@p_qtyDaily_{i}";
                string pDay = $"@p_day_{i}";

                sb.Append($"({pPartId}, {pDate}, {pQtyDaily}, {pIsoWeek}, {pDay})");
                if (i < dataList.Count - 1) sb.Append(",");
                sb.AppendLine();

                paramList.Add(new NpgsqlParameter(pPartId, partId));
                paramList.Add(new NpgsqlParameter(pDate, dateVal));
                paramList.Add(new NpgsqlParameter(pIsoWeek, isoWeek));
                paramList.Add(new NpgsqlParameter(pQtyDaily, qtyDaily));
                paramList.Add(new NpgsqlParameter(pDay, (object)(dayVal ?? "") ?? DBNull.Value));
            }

            sb.Append(";");
            string sql = sb.ToString();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(paramList.ToArray());

            int inserted = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"[DAO] InsertDailyPlansAtOnceAsync: {inserted} row(s) inserted.");
        }

        // 기존의 다른 메서드들(UpsertAllPlansAtOnceAsync, GetPlansByWeekAsync 등)은 필요에 따라 그대로 두거나
        // 사용하지 않을 경우 제거/주석 처리할 수 있다.

        public async Task<List<InjectionPlanDTO>> GetPlansByWeekAsync(int isoWeek)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                var param = new { iso_week = isoWeek };

                string sql = @"
                    SELECT
                        part_id    AS PartId,
                        date       AS Date,
                        qty_daily  AS QtyDaily,
                        iso_week   AS IsoWeek,
                        qty_weekly AS QtyWeekly,
                        day        AS Day
                    FROM injection_plan
                    WHERE iso_week = @iso_week
                    ORDER BY part_id, date
                ";

                var rows = await conn.QueryAsync<InjectionPlanDTO>(sql, param);
                return rows.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DAO] GetPlansByWeekAsync 에러: {ex.Message}");
                throw;
            }
        }
    }
}
