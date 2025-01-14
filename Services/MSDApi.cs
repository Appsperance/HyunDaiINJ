using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Services;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using HyunDaiINJ.Views.Login;

public class MSDApi
{
    private readonly HttpClient _client;

    public static string? JwtToken { get; private set; }
    public static string? JwtPublicKey { get; private set; }

    public MSDApi()
    {
        _client = new HttpClient();
    }

    public async Task<bool> LoginAsync(string userName, string password)
    {
        try
        {
            var loginModel = new { LoginId = userName, LoginPw = password };
            string jsonBody = JsonConvert.SerializeObject(loginModel);

            var requestContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("http://13.125.114.64:5282/api/login", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                string failReason = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[로그인 실패 - HTTP {response.StatusCode}] {failReason}");
                return false;
            }

            string responseData = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponseDTO>(responseData);
            if (result == null)
            {
                Console.WriteLine("[로그인 실패] 서버 응답(JSON)을 파싱할 수 없습니다.");
                return false;
            }

            Console.WriteLine($"[로그인 성공] ID={result.Id}, LoginId={result.LoginId}, JWT={result.JwtToken}");

            JwtToken = result.JwtToken;
            JwtPublicKey = result.JwtPublicKey;

            string role = JwtParser.ExtractRoleFromJwt(JwtToken);
            Console.WriteLine($"Extracted Role: {role}");

            if(role == "systemAdmin" || Regex.IsMatch(role, @"^admin\d*$", RegexOptions.IgnoreCase))
            {
                
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[로그인 중 예외 발생] " + ex.Message);
            return false;
        }
    }
}