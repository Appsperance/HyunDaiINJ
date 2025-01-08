using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.Queries
{
    class VisionCumQueries
    {
        public const string GetVisionData = @"
           WITH RankedData AS (
                SELECT
                    line_id,
                    time,
                    lot_id,
                    shift,
                    employee_number,
                    total,
                    ROW_NUMBER() OVER (PARTITION BY lot_id, DATE(time) ORDER BY time DESC) AS rnk
                FROM vision_cum
            )
            SELECT
                line_id,
                time,
                lot_id,
                shift,
                employee_number,
                total
            FROM RankedData
            WHERE rnk = 1
            ORDER BY lot_id, time ASC;";


    }
}
  
