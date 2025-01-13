using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;
using Newtonsoft.Json;  // 또는 System.Text.Json 사용 가능

namespace HyunDaiINJ.Services
{
    public class MSDApi
    {
        private readonly HttpClient _client;

        // 로그인 후 받은 JWT 토큰을 임시로 저장할 static 필드
        // ※ 실제 서비스에선 보안 이슈 등을 검토해야 합니다.
        public static string? JwtToken { get; private set; }

        public MSDApi()
        {
            _client = new HttpClient();
        }

        /// <summary>
        /// 로그인 API를 호출하고, 성공/실패를 bool로 반환
        /// 실패 시 서버에서 주는 원인 메시지(또는 상세 정보)는 Console에 출력
        /// </summary>
        public async Task<bool> LoginAsync(string userName, string password)
        {
            try
            {
                var loginModel = new { LoginId = userName, LoginPw = password };
                string jsonBody = JsonConvert.SerializeObject(loginModel);

                // Post
                var requestContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("http://localhost:5282/api/login", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    // 실패 - 상태 코드 200이 아닌 경우
                    string failReason = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[로그인 실패 - HTTP {response.StatusCode}] {failReason}");
                    return false;
                }

                // 여기까지 왔다면 status code == 200이라고 가정
                string responseData = await response.Content.ReadAsStringAsync();

                // 서버에서 내려주는 구조에 맞춰 역직렬화
                var result = JsonConvert.DeserializeObject<LoginResponseDTO>(responseData);
                if (result == null)
                {
                    Console.WriteLine("[로그인 실패] 서버 응답(JSON)을 파싱할 수 없습니다.");
                    return false;
                }

                // 필요한 후처리 (JWT 토큰 저장, 사용자 정보 저장 등)
                Console.WriteLine($"[로그인 성공] ID={result.Id}, LoginId={result.LoginId}, JWT={result.JwtToken}");

                // 로그인 성공 시, 토큰을 static 필드에 저장
                JwtToken = result.JwtToken;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[로그인 중 예외 발생] " + ex.Message);
                return false;
            }
        }
    }
}
