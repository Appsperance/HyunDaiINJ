using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyunDaiINJ.DATA.DTO
{
    public class LoginResponseDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("loginId")]
        public string LoginId { get; set; } = string.Empty;

        [JsonProperty("employeeNumber")]
        public long EmployeeNumber { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("photo")]
        public string Photo { get; set; } = string.Empty;

        [JsonProperty("shift")]
        public string Shift { get; set; } = string.Empty;

        [JsonProperty("jwtToken")]
        public string JwtToken { get; set; } = string.Empty;

        [JsonProperty("jwtPublicKey")]
        public string JwtPublicKey { get; set; } = string.Empty;
    }
}
