using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyunDaiINJ.DATA.DTO
{
    public class MqttProcessDTO
    {
        [JsonProperty("X20")]
        //운전시작
        public string? X20 { get; set; }

        [JsonProperty("X21")]
        //운전정지
        public string? X21 { get; set; }

        [JsonProperty("Y40")]
        //컨베이어벨트상태
        public string? Y40 { get; set; }

        [JsonProperty("D1")]
        //대기 - 주입전 / 취
        public string? D1 { get; set; }

        [JsonProperty("D2")]
        public string? D2 { get; set; }

        [JsonProperty("Y41")]
        // 주입공정
        public string? Y41 { get; set; }

        [JsonProperty("Y42")]
        // 성형공정
        public string? Y42 { get; set; }

        [JsonProperty("Y43")]
        // 취출 공정
        public string? Y43 { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
