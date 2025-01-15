using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyunDaiINJ.DATA.DTO
{
    public class InjectionPlanDTO
    {
        public string? PartId {  get; set; }
        public DateTime? Date { get; set; }
        public int? QtyDaily {  get; set; }
        public int? IsoWeek {  get; set; }
        public int? QtyWeekly { get; set; }
        public string? Day { get; set; }
    }
}
