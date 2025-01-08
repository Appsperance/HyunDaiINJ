using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.DTO
{
    public class VisionCumDTO
    {
        public string? lineId { get; set; }
        public DateTime time { get; set; }
        public string? lotId { get; set; }
        public string? shift { get; set; }
        public long? employeeNumber { get; set; }
        public int? total { get; set; }
    }
}
