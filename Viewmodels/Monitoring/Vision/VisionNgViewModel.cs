using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HyunDaiINJ.DATA.DTO;
using System.Collections.Generic;
using System.Linq;

public class VisionNgViewModel : INotifyPropertyChanged
{
    private MSDApi _api;

    // DataGrid에 바인딩할 컬렉션
    // ObservableCollection을 쓰면 아이템 추가/삭제 시 자동으로 UI가 갱신됩니다.
    private ObservableCollection<VisionNgDTO> _ngDetailedData;
    public ObservableCollection<VisionNgDTO> NgDetailedData
    {
        get => _ngDetailedData;
        set
        {
            _ngDetailedData = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<VisionNgDTO> _ngSummaryData;
    public ObservableCollection<VisionNgDTO> NgSummaryData
    {
        get => _ngSummaryData;
        set
        {
            _ngSummaryData = value;
            OnPropertyChanged();
        }
    }

    public VisionNgViewModel()
    {
        _api = new MSDApi();
        NgDetailedData = new ObservableCollection<VisionNgDTO>();
        NgSummaryData = new ObservableCollection<VisionNgDTO>();
    }

    // 예시: 서버 API에서 데이터 받아온 뒤, 불량 종류별로 GroupBy
    public void UpdateSummaryData(List<VisionNgDTO> dtoList)
    {
        NgSummaryData.Clear();

        // GroupBy, NotClassified → Good 치환 등 필요하다면 여기서
        var grouped = dtoList
            .GroupBy(dto => dto.DisplayNgLabel) // 또는 dto.NgLabel
            .Select(g => new VisionNgDTO
            {
                NgLabel = g.Key,
                LabelCount = g.Count()
            });

        foreach (var item in grouped)
        {
            NgSummaryData.Add(item);
        }
    }

    // 이 메서드가 서버 API 호출 후, 결과를 NgDetailedData에 담아줌
    public async Task LoadDataFromServerAsync()
    {
        try
        {
            // (선택) JWT 로그인 필요 시
            // bool loginOk = await _api.LoginAsync("ID", "Password");
            // if(!loginOk) { /* 에러 처리*/ }

            // API 호출 파라미터 (lineIds, offset, count)
            var lineIds = new List<string> { "vp01", "vi01", "vp03", "vp04", "vp05" };
            int offset = 0;
            int count = 100;

            // 서버로부터 데이터 가져오기
            var list = await _api.GetNgImagesAsync(lineIds, offset, count);

            if (list != null)
            {
                // 기존 데이터 지우고 새로 채움
                NgDetailedData.Clear();
                foreach (var item in list)
                {
                    NgDetailedData.Add(item);
                }

                // (중요) 여기서 바로 요약 데이터 업데이트
                UpdateSummaryData(NgDetailedData.ToList());
            }
        }
        catch (Exception ex)
        {
            // 로깅, 메시지 등
            Console.WriteLine($"LoadDataFromServerAsync 예외: {ex.Message}");
        }
    }

    // INotifyPropertyChanged 구현부
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
