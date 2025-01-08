using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.DTO
{
    public class VisionNgDTO
    {
        public int Id { get; set; } // serial4
        public string? LotId { get; set; } // varchar(20)
        public string? PartId { get; set; } // varchar(5)
        public string? LineId { get; set; } // varchar(10)
        public DateTime DateTime { get; set; } // timestamptz
        public string? NgLabel { get; set; } // varchar(50)
        public string? NgImgPath { get; set; } // text
    }
}
