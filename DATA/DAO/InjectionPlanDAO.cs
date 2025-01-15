using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using WpfApp1;                 // App.ConnectionString 위치
using HyunDaiINJ.DATA.Queries;
using HyunDaiINJ.Models.Plan;
using System.Text;  // DailyPlanModel

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
        /// 다중 row를 한 번에 INSERT (part_id, date, qty_daily=0, iso_week, qty_weekly, day)
        /// </summary>
        public async Task InsertAllPlansAtOnceAsync(
            List<(string name, DateTime dateVal, int isoWeek, int qtyWeekly, string dayVal)> dataList
        )
        {
            if (dataList == null || dataList.Count == 0)
                return;

            // 1) 쿼리 빌드
            //    INSERT INTO injection_plan (part_id, date, qty_daily, iso_week, qty_weekly, day)
            //    VALUES (@p_partId_0, @p_date_0, 0, @p_isoWeek_0, @p_qtyWk_0, @p_day_0),
            //           ...
            var sb = new StringBuilder();
            sb.AppendLine(
                "INSERT INTO injection_plan (part_id, date, qty_daily, iso_week, qty_weekly, day) VALUES"
            );

            var paramList = new List<NpgsqlParameter>();

            for (int i = 0; i < dataList.Count; i++)
            {
                var (partId, dateVal, isoWeek, qtyWeekly, dayVal) = dataList[i];

                string pPartId = $"@p_partId_{i}";
                string pDate = $"@p_date_{i}";
                string pIsoWeek = $"@p_isoWeek_{i}";
                string pQtyWeekly = $"@p_qtyWeekly_{i}";
                string pDay = $"@p_day_{i}";

                // qty_daily는 0 고정
                sb.Append($"({pPartId}, {pDate}, 0, {pIsoWeek}, {pQtyWeekly}, {pDay})");
                if (i < dataList.Count - 1) sb.Append(",");
                sb.AppendLine();

                // 파라미터 생성
                paramList.Add(new NpgsqlParameter(pPartId, partId));
                paramList.Add(new NpgsqlParameter(pDate, dateVal));     // date
                paramList.Add(new NpgsqlParameter(pIsoWeek, isoWeek));  // int
                paramList.Add(new NpgsqlParameter(pQtyWeekly, qtyWeekly)); // int
                paramList.Add(
                    new NpgsqlParameter(pDay, (object)(dayVal ?? "") ?? DBNull.Value)
                );
            }

            sb.Append(";"); // 구문 끝
            string sql = sb.ToString();

            // 2) DB 연결/실행
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(paramList.ToArray());

            int inserted = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"[DAO] InsertAllPlansAtOnceAsync: {inserted} row(s) inserted.");
        }

        /// <summary>
        /// 특정 주차(week)에 해당하는 DailyPlanModel 목록 조회
        /// </summary>
        // 주차별 SELECT
        public async Task<List<DailyPlanModel>> GetPlansByWeekAsync(int isoWeek)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                // param 이름 => new { iso_week = isoWeek }
                var param = new { iso_week = isoWeek };

                var rows = await conn.QueryAsync<DailyPlanModel>(
                    InjectionPlanQueries.SelectInjectionPlanWeekData,
                    param
                );

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
