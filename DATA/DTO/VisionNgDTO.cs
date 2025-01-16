using System;
using System.Collections.Generic;

namespace HyunDaiINJ.DATA.DTO
{
    public class VisionNgDTO
    {
        public int Id { get; set; } // serial4
        public string? LotId { get; set; } // varchar(20)
        public string? PartId { get; set; } // varchar(5)
        public string? LineId { get; set; } // varchar(10)
        public string? DateTime { get; set; } // timestamptz
        public string? NgLabel { get; set; } // varchar(50)
        public int LabelCount { get; set; }
        public string? NgImgPath { get; set; } // text

        public string NgImgBase64 { get; set; }

        // 주별 데이터 속성 추가
        public int YearNumber { get; set; } // 주별 연도
        public int WeekNumber { get; set; } // 주 번호
        public DateTime WeekStartDate { get; set; } // 주 시작 날짜
        public DateTime WeekEndDate { get; set; } // 주 끝 날짜
       
    }
}
