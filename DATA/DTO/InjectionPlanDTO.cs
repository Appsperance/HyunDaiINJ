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
        public int Week {  get; set; }
        public DateTime? Date { get; set; }
        public int? DailyQuan {  get; set; }
        public string? Days { get; set; }
        public int? WeekQuan { get; set; }

        public Dictionary<string, int> PartIdData { get; set; } = new Dictionary<string, int>();
    }
}
