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

            var response = await _client.PostAsync("http://13.125.114.64:5282/api/login",
                new StringContent(jsonBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                return false;

            string responseData = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponseDTO>(responseData);
            if (result == null)
                return false;

            // JWT 보관
            JwtToken = result.JwtToken;
            JwtPublicKey = result.JwtPublicKey;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<VisionNgDTO>> GetNgImagesAsync(List<string> lineIds, int offset, int count)
    {
        // 1) POST 요청
        var url = "http://13.125.114.64:5282/api/Vision/ng/images";
        var requestBody = new VisionNgImgPathReqDto
        {
            lineIds = lineIds,
            offset = offset,
            count = count
        };

        using (var request = new HttpRequestMessage(HttpMethod.Post, url))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", JwtToken);

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                // 에러 처리
                return null;
            }

            // 2) JSON 파싱 -> List<VisionNgImgPathRspDto>
            var content = await response.Content.ReadAsStringAsync();
            var lineArray = JsonConvert.DeserializeObject<List<VisionNgImgPathRspDto>>(content);

            // 3) 여러 라인의 visionNgImages를 합친다
            var allImages = new List<VisionNgDTO>();
            foreach (var lineData in lineArray)
            {
                if (lineData.visionNgImages != null)
                    allImages.AddRange(lineData.visionNgImages);
            }

            // allImages에 id, partId, dateTime, ...가 들어 있음
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
    public async Task<bool> SendWeekPlanAsync(string partId, int weekNumber, int qtyWeekly)
    {
        if (string.IsNullOrEmpty(JwtToken))
            throw new InvalidOperationException("로그인이 필요합니다. JWT 토큰이 없습니다.");

        // 전송할 JSON 바디
        var bodyObj = new
        {
            partId = partId,
            weekNumber = weekNumber,
            qtyWeekly = qtyWeekly
        };

        string jsonBody = JsonConvert.SerializeObject(bodyObj);

        // 예) POST http://13.125.114.64:5282/api/Plan/week
        using var request = new HttpRequestMessage(HttpMethod.Post, "http://13.125.114.64:5282/api/Plan/week");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JwtToken); // JWT 토큰 헤더
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            string errorMsg = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[SendWeekPlanAsync] 실패: {response.StatusCode}, {errorMsg}");
            return false;
        }

        return true;
    }

    public async Task<int> GetWeekNumberAsync(DateTime date)
    {
        // JWT 토큰 필요 시, Login 후 발급받아 저장해두어야 함.
        // 만약 해당 API가 JWT 인증이 필요 없다면, 굳이 토큰 체크가 없어도 됨.
        // if (string.IsNullOrEmpty(JwtToken))
        //     throw new InvalidOperationException("로그인이 필요합니다. JWT 토큰이 없습니다.");

        try
        {
            using HttpClient client = new HttpClient();

            // 서버에서 요구하는 날짜 형식 (예: yyyy-MM-dd)
            string formattedDate = date.ToString("yyyy-MM-dd");
            string requestUrl = $"http://13.125.114.64:5282/api/calendar/week-number/{formattedDate}";
            Console.WriteLine($"[GetWeekNumberInfoAsync] 요청 날짜: {formattedDate}");
            

            // GET 요청
            HttpResponseMessage response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            // 응답을 문자열로
            string jsonString = await response.Content.ReadAsStringAsync();
            // 주차 번호가 정수라고 가정
            int receivedWeekNumber = int.Parse(jsonString);

            return receivedWeekNumber;
        }
        catch (Exception ex)
        {
            // 상황에 맞게 예외 처리
            Console.WriteLine($"[GetWeekNumberAsync] 에러: {ex.Message}");
            return -1; // 오류 시, 특정 값을 리턴
        }

    }

    public async Task<WeekNumberInfo> GetWeekNumberInfoAsync(DateTime date)
    {
        try
        {
            string formattedDate = date.ToString("yyyy-MM-dd");
            // 서버 URL: 예시로 작성. 실제 값에 맞춰 수정
            string requestUrl = $"http://13.125.114.64:5282/api/calendar/week-number/{formattedDate}";

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                // {"weekNumber":3, "dates":["2025-01-13T00:00:00", ...]} 구조
                var info = JsonConvert.DeserializeObject<WeekNumberInfo>(jsonString);
                Console.WriteLine($"[GetWeekNumberInfoAsync] 서버 응답: {jsonString}");
                return info;


            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MSDApi] GetWeekNumberInfoAsync 에러: {ex.Message}");
            return null;
        }
    }


}
