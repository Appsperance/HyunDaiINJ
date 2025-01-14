using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HyunDaiINJ.Services
{
    public static class JwtParser
    {
        public static string ExtractRoleFromJwt(string rawJwt)
        {
            try
            {
                string[] parts = rawJwt.Split('.');

                if (parts.Length < 2)
                    throw new ArgumentException("Invalid JWT format. Cannot extract payload.");

                // Base64 디코딩
                string payloadBase64 = parts[1];
                string payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(payloadBase64)));

                // JSON 파싱
                JObject payload = JObject.Parse(payloadJson);

                // 복잡한 경로로 role 값 추출
                string roleKey = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
                if (payload.TryGetValue(roleKey, out JToken? roleToken))
                {
                    return roleToken?.ToString() ?? "Role not found in payload.";
                }

                return "Role not found in payload.";
            }
            catch (Exception ex)
            {
                return $"Error extracting role: {ex.Message}";
            }
        }

        private static string PadBase64(string base64)
        {
            int padding = 4 - (base64.Length % 4);
            if (padding < 4)
            {
                base64 += new string('=', padding);
            }
            return base64;
        }
    }
}
