using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.Queries
{
    public static class VisionNgQueries
    {
        public const string GetVisionNgData = @"
            SELECT  ng_label, COUNT(*) AS label_count
            FROM vision_ng
            GROUP BY  ng_label
            ORDER BY  ng_label;";

        public const string GetVisionNgDataAll = @"
            SELECT  * FROM vision_ng;";
    }
}
