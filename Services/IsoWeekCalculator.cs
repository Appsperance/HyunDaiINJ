using System;
using System.Globalization;

namespace HyunDaiINJ.Services
{
    public static class IsoWeekCalculator
    {
        // 주어진 날짜의 ISO 8601 주차 번호를 구하는 메서드(이미 있다고 하셨음)
        public static int GetIso8601WeekOfYear(DateTime date)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            Calendar cal = ci.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            return cal.GetWeekOfYear(date, weekRule, firstDayOfWeek);
        }

        // 추가: ISO 8601 기준으로 "year년의 weekNum번째 주차"가 시작되는 (월요일) 날짜 구하기
        public static DateTime FirstDayOfIsoWeek(int year, int weekNum)
        {
            // ISO-8601 규칙상 "해당 연도의 첫 주는 1월4일이 속한 주"
            // 아래 로직은 흔히 StackOverflow 등에서 많이 소개된 방법입니다.

            // 1) 1월 4일
            DateTime jan4 = new DateTime(year, 1, 4);

            // 2) 그 1월 4일이 속한 ISO 주차의 "년도"
            int jan4Week = GetIso8601WeekOfYear(jan4);

            // ISO 주차 1의 월요일 찾기 위해...
            // '해당 year's first Thursday'를 구하는 테크닉도 있고,
            // 여기서는 조금 더 직관적인 방식을 쓸 수 있습니다.

            // 아래는 대표적 구현 (참고 용)
            DateTime firstWeek = GetWeekStart(year, 1);
            // "year의 1주차 월요일"을 가져온 후, (weekNum-1) * 7일을 더하기

            return firstWeek.AddDays((weekNum - 1) * 7);
        }

        // "year년, isoWeek=1"일 때의 월요일(주 시작) 구하기
        // 간단 구현 예시 (실제는 더 복잡한 ISO 규칙을 고려해야 할 수도 있음)
        private static DateTime GetWeekStart(int year, int isoWeek)
        {
            // 그냥 "1월 4일"을 기준으로 "해당 isoWeek의 월요일"을 역산
            // 또는 "해당년도 1월1일 ~ 1월7일 사이"에서 "FirstFourDayWeek" 규칙에 맞는 월요일 찾기 등
            // 여기서는 가장 직관적인 방법 중 하나만 예시로 작성:

            // 1) 1월4일이 속한 ISO주차를 구해서, 그 주차의 월요일을 찾고
            //    "그게 해당 연도의 1주차 월요일"이라고 가정
            DateTime jan4 = new DateTime(year, 1, 4);
            int w = GetIso8601WeekOfYear(jan4); // 예: w=1 or 53 등
            // 만약 w != 1이면, 한두 주 빼거나 더해야 할 수도 있음

            // 실제로는 좀 복잡하지만, 일단 "jan4"의 주차를 찾아서 "월요일"로 보정
            // 여기서는 간단히: jan4 - (jan4.DayOfWeek-월요일) = 그 주의 월요일
            // 다만 DayOfWeek가 Sunday=0, Monday=1, ... 이므로 로직 보완 필요

            // 여기서는 직접 구현보다는, 대표적으로 알려진 코드를 간단히 붙여드리는 게 나을 수 있습니다.
            // 편의상 아래처럼 구현 (실무에서는 더 철저히 검증해야 합니다)

            DateTime thursdayOfFirstWeek = GetThursdayOfFirstIsoWeek(year);
            // year의 "첫 ISO 주차에 속한 목요일"
            // 그리고 "목요일 - 3일" 하면 월요일
            var mondayOfFirstWeek = thursdayOfFirstWeek.AddDays(-3);
            return mondayOfFirstWeek;
        }

        // year의 첫 ISO 주차에 속한 "목요일" 구하기
        private static DateTime GetThursdayOfFirstIsoWeek(int year)
        {
            // ISO 기준, "첫 주는 1월 4일이 속한 주"를 구하는 표준 테크닉
            var jan1 = new DateTime(year, 1, 1);
            // '첫 목요일' 찾기
            int offset = (int)DayOfWeek.Thursday - (int)jan1.DayOfWeek;
            if (offset < 0) offset += 7;
            return jan1.AddDays(offset);
        }
    }
}
