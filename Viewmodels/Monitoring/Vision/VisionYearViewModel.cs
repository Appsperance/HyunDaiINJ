using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.ViewModels.Monitoring.Vision
{
    public class VisionYearViewModel : INotifyPropertyChanged
    {
        private readonly MSDApi _api;

        // 10초마다 LoadVisionNgDataYearAsync()를 호출할 타이머
        private DispatcherTimer _timer;

        // 서버에서 불러온 원본 DTO
        private ObservableCollection<VisionNgDTO> _yearData;
        public ObservableCollection<VisionNgDTO> YearData
        {
            get => _yearData;
            set
            {
                _yearData = value;
                OnPropertyChanged();
            }
        }

        // (A) "년도+라벨"로 묶어 Count() 계산한 결과
        private ObservableCollection<VisionNgDTO> _yearLabelSummaries;
        public ObservableCollection<VisionNgDTO> YearLabelSummaries
        {
            get => _yearLabelSummaries;
            set
            {
                _yearLabelSummaries = value;
                OnPropertyChanged();
            }
        }

        public VisionYearViewModel()
        {
            _api = new MSDApi();
            YearData = new ObservableCollection<VisionNgDTO>();
            YearLabelSummaries = new ObservableCollection<VisionNgDTO>();

            // (1) 10초 간격 DispatcherTimer 설정
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
                
            };
            _timer.Tick += async (s, e) =>
            {
                Console.WriteLine("[VisionYearViewModel] Timer Tick event fired. (Before API call)");
                try
                {
                    await LoadVisionNgDataYearAsync();
                    Console.WriteLine("[VisionYearViewModel] Timer Tick event fired. (After API call)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[VisionYearViewModel] Tick event EXCEPTION: " + ex.Message);
                }
            };

            _timer.Start();
        }

        /// <summary>
        /// 서버에서 데이터 불러온 뒤: (1) YearNumber 세팅, (2) GroupBy → Count() 
        /// </summary>
        public async Task LoadVisionNgDataYearAsync()
        {
            try
            {
                var lineIds = new List<string> { "vp01", "vi01", "vp03", "vp04", "vp05" };
                int offset = 0;
                int count = 500;

                var dtoList = await _api.GetNgImagesAsync(lineIds, offset, count);
                if (dtoList == null)
                {
                    YearData.Clear();
                    YearLabelSummaries.Clear();
                    return;
                }

                // 기존 데이터 초기화
                YearData.Clear();

                // (1) DateTime → YearNumber
                foreach (var d in dtoList)
                {
                    if (!string.IsNullOrEmpty(d.DateTime))
                    {
                        if (DateTime.TryParse(d.DateTime, out var parsedDt))
                        {
                            d.YearNumber = parsedDt.Year;
                        }
                    }
                    YearData.Add(d);
                }

                // (2) GroupBy (년도+라벨) → Count
                BuildYearLabelSummaries();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadVisionNgDataYearAsync] error: {ex.Message}");
            }
        }

        private void BuildYearLabelSummaries()
        {
            var grouped = YearData
                .GroupBy(d => new { d.YearNumber, d.NgLabel })
                .Select(g => new VisionNgDTO
                {
                    YearNumber = g.Key.YearNumber,
                    NgLabel = g.Key.NgLabel,
                    LabelCount = g.Count()
                })
                .ToList();

            // 재할당 -> OnPropertyChanged("YearLabelSummaries") 발생
            YearLabelSummaries = new ObservableCollection<VisionNgDTO>(grouped);
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
