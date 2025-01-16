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
using System.Collections.Generic;
using System.Net.Http.Headers;

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

            if (role == "systemAdmin" || Regex.IsMatch(role, @"^admin\d*$", RegexOptions.IgnoreCase))
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
    public async Task<List<VisionNgDTO>> GetNgImagesAsync(List<string> lineIds, int offset, int count)
    {
        if (string.IsNullOrEmpty(JwtToken))
        {
            throw new InvalidOperationException("JWT 토큰이 없습니다. 먼저 로그인하세요.");
        }

        var url = "http://13.125.114.64:5282/api/Vision/ng/images";
        var requestBody = new VisionNgImgPathReqDto
        {
            lineIds = lineIds,
            offset = offset,
            count = count
        };

        using (var request = new HttpRequestMessage(HttpMethod.Post, url))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
            string jsonBody = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[NG 이미지 조회 실패] Status={response.StatusCode}, Body={err}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            // 서버 응답이 최상위 배열 [ { lineId, visionNgImages: [...] }, ... ]
            var responseList = JsonConvert.DeserializeObject<List<VisionNgImgPathRspDto>>(content);
            if (responseList == null)
            {
                return null;
            }

            // 여러 lineId에서 온 NG 이미지를 전부 합쳐서 반환
            var allImages = new List<VisionNgDTO>();
            foreach (var lineData in responseList)
            {
                if (lineData.visionNgImages != null)
                    allImages.AddRange(lineData.visionNgImages);
            }

            // 반환된 NG 이미지들의 ngImgPath는 이미 서버가 /home/... -> http://... 등으로 변환했다고 가정
            // (즉, 날짜 폴더를 자동으로 붙여 HTTP URL을 만들어주는 로직은 서버 내부에서 처리)
            return allImages;
        }
    }
    public async Task<List<VisionNgDTO>> GetNgImagesByIdsAsync(List<int> ids)
    {
        if (string.IsNullOrEmpty(JwtToken))
            throw new InvalidOperationException("JWT 토큰이 없습니다.");

        // ids=[33, 32]를 -> "33,32"로 만든다.
        string idParams = string.Join(",", ids);

        string url = $"http://13.125.114.64:5282/api/Vision/ng/images?ids={Uri.EscapeDataString(idParams)}";

        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[NG 이미지 조회 실패] Status={response.StatusCode}, Body={err}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            // 응답이 배열이므로 List<VisionNgDTO>로 역직렬화
            var ngList = JsonConvert.DeserializeObject<List<VisionNgDTO>>(content);
            return ngList;
        }
    }



}