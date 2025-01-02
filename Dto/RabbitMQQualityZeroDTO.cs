using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyunDaiINJ.Dto
{
    public class RabbitMQQualityZeroDTO
    {
        [JsonProperty("productName")]
        public string? ProductName { get; set; }

        [JsonProperty("lotNumber")]
        public string? LotNumber { get; set; }

        [JsonProperty("productId")]
        public string? ProductId { get; set; }

        [JsonProperty("lineId")]
        public string? LineId { get; set; }

        [JsonProperty("operatorName")]
        public string? OperatorName { get; set; }
    }
}
