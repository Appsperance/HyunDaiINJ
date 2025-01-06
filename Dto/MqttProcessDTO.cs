using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyunDaiINJ.DTO
{
    public class MqttProcessDTO
    {
        [JsonProperty("X20")]
        public string? X20 { get; set; }

        [JsonProperty("X21")]
        public string? X21 { get; set; }

        [JsonProperty("Y40")]
        public string? Y40 { get; set; }

        [JsonProperty("D1")]
        public string? D1 { get; set; }

        [JsonProperty("D2")]
        public string? D2 { get; set; }

        [JsonProperty("Y41")]
        public string? Y41 { get; set; }

        [JsonProperty("Y42")]
        public string? Y42 { get; set; }

        [JsonProperty("Y43")]
        public string? Y43 { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
