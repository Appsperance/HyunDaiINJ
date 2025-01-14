using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.Queries
{
    public static class VisionNgQueries
    {
        public const string GetVisionNgDataAll = @"
           SELECT 
                id,
                lot_id,
                part_id,
                line_id,
                to_char(
                     date_time AT TIME ZONE 'Asia/Seoul',
                     'YYYY-MM-DD HH24:MI:SS'
                   ) AS date_str,
                ng_label,
                ng_img_path
            FROM vision_ng;";

        public const string GetVisionNgData = @"
            SELECT  ng_label, COUNT(*) AS label_count
            FROM vision_ng
            GROUP BY  ng_label
            ORDER BY  ng_label;";

        public const string GetVisionNgDataWeek = @"
            SELECT 
                DATE_PART('year', date_time)::INTEGER AS year_number,        -- 연도를 정수로 변환
                DATE_PART('week', date_time)::INTEGER AS week_number,        -- 주 번호를 정수로 변환
                MIN(date_time) AS week_start_date,                          -- 주 시작 날짜
                MAX(date_time) AS week_end_date,                            -- 주 끝 날짜
                ng_label,                                                   -- NG 라벨
                COUNT(*)::INTEGER AS ng_count                               -- NG 발생 횟수를 정수로 변환
            FROM vision_ng
            GROUP BY year_number, week_number, ng_label
            ORDER BY year_number, week_number, ng_label;";

        public const string GetVisionNgDataDaily= @"
            SELECT ng_label,
                   COUNT(*) AS label_count
            FROM vision_ng
            WHERE date_time::date = CURRENT_DATE
            GROUP BY ng_label;";

        public const string GetVisionNgDataYear = @"
             SELECT
             EXTRACT(YEAR FROM date_time) AS year_num,
             ng_label,
             COUNT(*) AS label_count
             FROM vision_ng
             GROUP BY year_num, ng_label
             ORDER BY year_num, ng_label;";

       
    }
}
