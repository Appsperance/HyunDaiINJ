using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyunDaiINJ.DATA.DTO
{
    public class VisionCumDTO
    {
        [JsonProperty("line_id")]
        public string? lineId { get; set; }

        [JsonProperty("time")]
        public DateTime time { get; set; }
        [JsonProperty("lot_id")]
        public string? lotId { get; set; }
        [JsonProperty("shift")]
        public string? shift { get; set; }
        [JsonProperty("employee_number")]
        public long? employeeNumber { get; set; }
        [JsonProperty("total")]
        public int? total { get; set; }
    }
}
