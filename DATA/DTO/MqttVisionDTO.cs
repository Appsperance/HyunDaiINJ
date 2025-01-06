using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyunDaiINJ.DATA.DTO
{
    public class MqttVisionDTO
    {
        [JsonProperty("lineId")]
        public string? LineId { get; set; }

        [JsonProperty("lotId")]
        public string? LotId { get; set; }

        [JsonProperty("shift")]
        public string? Shift { get; set; }

        [JsonProperty("employeeNumber")]
        public string? EmployeeNumber { get; set; }

        [JsonProperty("ngImg")]
        public byte[]? NgImg { get; set; }

        [JsonProperty("stageVal")]
        public string? StageVal { get; set; }

    }

}
