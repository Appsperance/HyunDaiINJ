using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.Services
{
    public class IsoWeekCalculator
    {
        public static int GetIso8601WeekOfYear(DateTime date)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            Calendar cal = ci.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            return cal.GetWeekOfYear(date, weekRule, firstDayOfWeek);
        }
    }
}
