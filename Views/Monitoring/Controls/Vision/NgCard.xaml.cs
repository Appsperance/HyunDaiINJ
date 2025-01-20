using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Services;
using System.Diagnostics; // Debug.WriteLine

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class NgCard : Window
    {
        private MSDApi _api;
        private List<VisionNgDTO> _cachedList;

        public NgCard()
        {
            InitializeComponent();
            InitializeNgCard();
            _api = new MSDApi();

            // 윈도우 생성자에서 로그
            Debug.WriteLine("[NgCard] 생성자 호출됨");
            MessageBox.Show("[생성자 호출됨] NgCard 생성자를 실행했습니다.");

            this.SizeChanged += NgCard_SizeChanged;
        }

        private void NgCard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newWidth = e.NewSize.Width;
            double newHeight = e.NewSize.Height;
            Debug.WriteLine($"[NgCard_SizeChanged] New Width={newWidth}, New Height={newHeight}");
        }

        private void InitializeNgCard()
        {
            this.SizeChanged += NgCard_SizeChanged;
        }

        public NgCard(ImageSource imageSource)
        {
            InitializeComponent();
            InitializeNgCard();
            _api = new MSDApi();

            Debug.WriteLine("[NgCard] 생성자(이미지) 호출됨");
            SelectedImage.Source = imageSource;
        }

        private async void NgCard_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[NgCard_Loaded] 윈도우 로드 시작");
            try
            {
                // 1) 목록 API 호출 -> id, ngImgPath, etc.만 받음 (ngImgBase64는 null)
                var lineIds = new List<string> { "vi01" };
                int offset = 2;
                int count = 10;

                Debug.WriteLine($"[NgCard_Loaded] GetNgImagesAsync(lineIds={string.Join(",", lineIds)}, offset={offset}, count={count}) 호출");
                var ngImages = await _api.GetNgImagesAsync(lineIds, offset, count);

                if (ngImages == null)
                {
                    Debug.WriteLine("[NgCard_Loaded] ngImages == null");
                    MessageBox.Show("데이터 없음 (null)");
                    return;
                }
                if (ngImages.Count == 0)
                {
                    Debug.WriteLine("[NgCard_Loaded] ngImages.Count == 0");
                    MessageBox.Show("데이터 없음 (0개)");
                    return;
                }

                Debug.WriteLine($"[NgCard_Loaded] 목록 API 결과 {ngImages.Count}개");

                // 2) 각 항목에 대해 상세 API 호출 -> Base64 채운다
                foreach (var dto in ngImages)
                {
                    Debug.WriteLine($"[NgCard_Loaded] 상세조회 시도 -> id={dto.Id}");
                    var detailList = await _api.GetNgImagesByIdsAsync(new List<int> { dto.Id });
                    if (detailList == null)
                    {
                        Debug.WriteLine($"[NgCard_Loaded] detailList == null (id={dto.Id})");
                        continue;
                    }
                    if (detailList.Count == 0)
                    {
                        Debug.WriteLine($"[NgCard_Loaded] detailList.Count=0 (id={dto.Id})");
                        continue;
                    }

                    var detailDto = detailList[0];
                    Debug.WriteLine($"[NgCard_Loaded] detailDto: id={detailDto.Id}, base64 length={detailDto.NgImgBase64?.Length}");

                    // 첫 번째 결과의 ngImgBase64를 dto에 저장
                    dto.NgImgBase64 = detailDto.NgImgBase64;
                }

                // 3) ListView 바인딩
                _cachedList = ngImages;
                ImageListView.ItemsSource = _cachedList;

                Debug.WriteLine("[NgCard_Loaded] 최종 ListView 바인딩 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NgCard_Loaded] 예외 발생: {ex}");
                MessageBox.Show("오류: " + ex.Message);
            }
        }

        private void ImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListView.SelectedItem is VisionNgDTO selectedDto)
            {
                Debug.WriteLine($"[ImageListView_SelectionChanged] 선택됨 -> id={selectedDto.Id}, base64 length={selectedDto.NgImgBase64?.Length}");

                // 이미 ngImgBase64가 채워져 있다고 가정
                if (!string.IsNullOrEmpty(selectedDto.NgImgBase64))
                {
                    var bmp = Base64ImageHelper.ConvertBase64ToBitmapImage(selectedDto.NgImgBase64);
                    if (bmp != null)
                    {
                        SelectedImage.Source = bmp;
                        Debug.WriteLine("[ImageListView_SelectionChanged] 이미지 로드 성공");
                    }
                    else
                    {
                        Debug.WriteLine("[ImageListView_SelectionChanged] Base64 디코딩 실패");
                        MessageBox.Show("Base64 디코딩 실패");
                    }
                }
                else
                {
                    Debug.WriteLine("[ImageListView_SelectionChanged] ngImgBase64가 비어있음");
                    MessageBox.Show("ngImgBase64가 비어있음");
                }
            }
        }
    }
}
