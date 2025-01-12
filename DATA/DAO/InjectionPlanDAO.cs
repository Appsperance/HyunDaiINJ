using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using WpfApp1;                 // App.ConnectionString 위치
using HyunDaiINJ.DATA.Queries;
using HyunDaiINJ.Models.Plan;  // DailyPlanModel

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
        /// 특정 주차(week)에 해당하는 DailyPlanModel 목록 조회
        /// </summary>
        public async Task<List<DailyPlanModel>> GetPlansByWeekAsync(int week)
        {
            try
            {
                Console.WriteLine("[DAO] GetPlansByWeekAsync 시작");

                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                Console.WriteLine("[DAO] DB 연결 성공");

                var param = new { Week = week };
                var rows = await conn.QueryAsync<DailyPlanModel>(
                    InjectionPlanQueries.SelectInjectionPlanWeekData,
                    param
                );

                int count = rows.Count();
                Console.WriteLine($"[DAO] 주차={week}, 조회 건수={count}");

                return rows.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DAO] GetPlansByWeekAsync 에러: {ex.Message}");
                throw;
            }
        }

        // InsertPlan, UpdatePlan 등 필요한 추가 메서드를 동일 패턴으로 작성
    }
}
