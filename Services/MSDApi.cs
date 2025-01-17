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
    // 월~일 → 0~6 매핑용 딕셔너리
   
    public MSDApi()
    {
        _client = new HttpClient();
    }
    // PUT 요청으로 주차별 일일 생산 계획 수정
    public async Task<bool> UpdateDailyPlanAsync(int isoWeek, string partId, Dictionary<string, int> dailyDict)
    {
        var arrayToSend = new List<int>
    {
        dailyDict["월"],
        dailyDict["화"],
        dailyDict["수"],
        dailyDict["목"],
        dailyDict["금"],
        dailyDict["토"],
        dailyDict["일"]
    };

        // 1) JSON 직렬화 시 **배열만** 직렬화
        //    (즉, { partId=..., dailyQuantities=[...] } 이런게 아니라
        //     그냥 [0,1,2,3,4,5,6] 이라는 “배열”이 JSON 최상위)
        string jsonBody = JsonConvert.SerializeObject(arrayToSend);
        // 예: [700,100,0,0,0,0,0]

        // 2) URL은 /api/Plan/week/{weekNumber}?partId=xxx 등
        string url = $"http://13.125.114.64:5282/api/Plan/week/{isoWeek}?partId={partId}";

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtToken);
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            string errorMsg = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[UpdateDailyPlanAsync] 실패: {response.StatusCode}, {errorMsg}");
            return false;
        }

        Console.WriteLine($"[UpdateDailyPlanAsync] 성공: {partId}, isoWeek={isoWeek}");
        return true;
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

    /// <summary>
    /// 주차별 계획 데이터를 API에서 GET
    /// 예: GET http://13.125.114.64:5282/api/Plan/week/3
    /// → List<PlanWeekResponseDto> 형태의 JSON 응답
    /// </summary>
    public async Task<List<InjectionPlanDTO>> GetPlanWeekDataAsync(int weekNumber)
    {
        // JWT 필요 시 설정 (예시)
        if (!string.IsNullOrEmpty(JwtToken))
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", JwtToken);
        }

        // (추가) 주차 번호 확인 로깅
        Console.WriteLine($"[GetPlanWeekDataAsync] 호출 - 주차={weekNumber}");

        string url = $"http://13.125.114.64:5282/api/Plan/week/{weekNumber}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // (추가) 요청 URL 확인
        Console.WriteLine($"[GetPlanWeekDataAsync] 요청 URL: {url}");

        var response = await _client.SendAsync(request);

        // (추가) 응답 코드 로깅
        Console.WriteLine($"[GetPlanWeekDataAsync] 응답 코드: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[GetPlanWeekDataAsync] 실패: {response.StatusCode}, {errBody}");
            return null;
        }

        // JSON 파싱
        string json = await response.Content.ReadAsStringAsync();
        var planList = JsonConvert.DeserializeObject<List<InjectionPlanDTO>>(json);

        Console.WriteLine("[GetPlanWeekDataAsync] 수신 데이터 원문:");
        Console.WriteLine(json);

        // (추가) 받은 데이터 개수와 일부 항목 로깅
        if (planList != null)
        {
            Console.WriteLine($"[GetPlanWeekDataAsync] planList.Count = {planList.Count}");
            // 필요 시 planList 첫 항목(또는 전체 항목)도 출력
            if (planList.Count > 0)
            {
                var first = planList[0];
                Console.WriteLine($"[GetPlanWeekDataAsync] 예시 첫 항목: partId={first.PartId}, day={first.Day}, qty={first.QtyDaily}");
            }
        }
        else
        {
            Console.WriteLine("[GetPlanWeekDataAsync] planList가 null입니다.");
        }

        return planList;
    }



}
