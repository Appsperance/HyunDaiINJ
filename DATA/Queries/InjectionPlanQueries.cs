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
    }
}
