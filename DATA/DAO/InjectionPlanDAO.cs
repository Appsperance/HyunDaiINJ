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

        public InjectionPlanDAO()
        {
            _connectionString = App.ConnectionString
                ?? throw new InvalidOperationException("ConnectionString not initialized.");

            Console.WriteLine($"[DAO] InjectionPlanDAO 생성, ConnectionString={_connectionString}");
        }

        /// <summary>
        /// 다중 row를 한 번에 INSERT 
        /// (part_id, date, qty_daily=0, iso_week, qty_weekly, day)
        /// </summary>
        public async Task InsertAllPlansAtOnceAsync(
            List<(string name, DateTime dateVal, int isoWeek, int qtyWeekly, string dayVal)> dataList
        )
        {
            if (dataList == null || dataList.Count == 0)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("INSERT INTO injection_plan (part_id, date, qty_daily, iso_week, qty_weekly, day) VALUES");

            var paramList = new List<NpgsqlParameter>();

            for (int i = 0; i < dataList.Count; i++)
            {
                var (partId, dateVal, isoWeek, qtyWeekly, dayVal) = dataList[i];

                string pPartId = $"@p_partId_{i}";
                string pDate = $"@p_date_{i}";
                string pIsoWeek = $"@p_isoWeek_{i}";
                string pQtyWeekly = $"@p_qtyWeekly_{i}";
                string pDay = $"@p_day_{i}";

                sb.Append($"({pPartId}, {pDate}, 0, {pIsoWeek}, {pQtyWeekly}, {pDay})");
                if (i < dataList.Count - 1) sb.Append(",");
                sb.AppendLine();

                paramList.Add(new NpgsqlParameter(pPartId, partId));
                paramList.Add(new NpgsqlParameter(pDate, dateVal));
                paramList.Add(new NpgsqlParameter(pIsoWeek, isoWeek));
                paramList.Add(new NpgsqlParameter(pQtyWeekly, qtyWeekly));
                paramList.Add(new NpgsqlParameter(pDay, (object)(dayVal ?? "") ?? DBNull.Value));
            }

            sb.Append(";"); // 구문 끝
            string sql = sb.ToString();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(paramList.ToArray());

            int inserted = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"[DAO] InsertAllPlansAtOnceAsync: {inserted} row(s) inserted.");
        }

        /// <summary>
        /// 주차(week)에 해당하는 InjectionPlanDTO 목록 조회
        /// (DB 원본 레코드 그대로)
        /// </summary>
        public async Task<List<InjectionPlanDTO>> GetPlansByWeekAsync(int isoWeek)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                var param = new { iso_week = isoWeek };

                // 예시 Query. 실제로 "day" 컬럼이 있으면 아래처럼 매핑
                // 없으면 date -> "월/화..." 변환 필요
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

        /// <summary>
        /// 한 번에 N행 Upsert(INSERT ... ON CONFLICT...)
        /// </summary>
        public async Task UpsertAllPlansAtOnceAsync(
            List<(string partId, DateTime dateVal, int isoWeek, int qtyDaily, int qtyWeekly, string dayVal)> dataList
        )
        {
            if (dataList == null || dataList.Count == 0)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("INSERT INTO injection_plan (part_id, date, iso_week, qty_daily, qty_weekly, day)");
            sb.AppendLine("VALUES");

            var paramList = new List<NpgsqlParameter>();

            for (int i = 0; i < dataList.Count; i++)
            {
                var (partId, dateVal, isoWeek, qtyDaily, qtyWeekly, dayVal) = dataList[i];

                string pPartId = $"@p_partId_{i}";
                string pDate = $"@p_date_{i}";
                string pIsoWeek = $"@p_isoWeek_{i}";
                string pQtyDaily = $"@p_qtyDaily_{i}";
                string pQtyWeekly = $"@p_qtyWeekly_{i}";
                string pDayVal = $"@p_dayVal_{i}";

                sb.Append($"({pPartId}, {pDate}, {pIsoWeek}, {pQtyDaily}, {pQtyWeekly}, {pDayVal})");
                if (i < dataList.Count - 1) sb.Append(",");
                sb.AppendLine();

                paramList.Add(new NpgsqlParameter(pPartId, partId));
                paramList.Add(new NpgsqlParameter(pDate, dateVal));
                paramList.Add(new NpgsqlParameter(pIsoWeek, isoWeek));
                paramList.Add(new NpgsqlParameter(pQtyDaily, qtyDaily));
                paramList.Add(new NpgsqlParameter(pQtyWeekly, qtyWeekly));
                paramList.Add(new NpgsqlParameter(pDayVal, (object)(dayVal ?? "") ?? DBNull.Value));
            }

            sb.AppendLine(@"
                ON CONFLICT (part_id, date)
                DO UPDATE
                   SET qty_daily  = EXCLUDED.qty_daily,
                       iso_week   = EXCLUDED.iso_week,
                       qty_weekly = EXCLUDED.qty_weekly,
                       day        = EXCLUDED.day
                ;
            ");

            string sql = sb.ToString();

            Console.WriteLine($"[DAO] UpsertAllPlansAtOnceAsync SQL:\n{sql}");

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(paramList.ToArray());

            int affected = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"[DAO] UpsertAllPlansAtOnceAsync: {affected} row(s) affected.");
        }
    }
}
