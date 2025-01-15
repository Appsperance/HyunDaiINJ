using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.Queries
{
    public static class InjectionPlanQueries
    {
        // iso_week를 받아서 해당 주차 데이터 조회
        public const string SelectInjectionPlanWeekData = @"
                SELECT part_id, date, qty_daily, qty_weekly
                FROM injection_plan
                WHERE iso_week = @iso_week
                ORDER BY part_id";

        // 2) Upsert(INSERT ... ON CONFLICT) 쿼리 예시
        //    - 이미 (part_id, date)가 있으면 qty_daily만 UPDATE
        //    - 없으면 INSERT
        public const string UpsertInjectionPlan = @"
            INSERT INTO injection_plan (part_id, date, iso_week, qty_daily, qty_weekly, day)
            VALUES (@PartId, @DateVal, @IsoWeek, @QtyDaily, @QtyWeekly, @DayVal)
            ON CONFLICT (part_id, date)
            DO UPDATE
               SET 
                   qty_daily = EXCLUDED.qty_daily
                   -- 아래처럼 다른 컬럼도 함께 업데이트하고 싶으면 추가:
                   -- iso_week  = EXCLUDED.iso_week,
                   -- qty_weekly= EXCLUDED.qty_weekly,
                   -- day       = EXCLUDED.day
            ;
        ";
    }
}
