using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.Queries
{
    public static class InjectionPlanQueries
    {
        public const string SaveInjectionPlanAsync = @"
                INSERT INTO injection_plan (part_id, week, week_quan)
                VALUES (@PartId, @Week, @WeekQuan)";

        public const string SelectInjectionPlanWeekData = @"
                SELECT part_id, week, week_quan
                  FROM injection_plan
                 WHERE week = @Week
                 ORDER BY part_id";
    }
}
