using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.Services;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class NgCard : Window
    {
        private MSDApi _api;
        private List<VisionNgDTO> _cachedList;  // 전체 목록 캐싱

        public NgCard()
        {
            InitializeComponent();
            InitializeNgCard();
            _api = new MSDApi();
            this.SizeChanged += NgCard_SizeChanged;

            MessageBox.Show("[생성자 호출됨] NgCard 생성자를 실행했습니다.");
        }

        private void NgCard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newWidth = e.NewSize.Width;
            double newHeight = e.NewSize.Height;
            Console.WriteLine($"[SizeChanged] New Width={newWidth}, New Height={newHeight}");
        }

        private void InitializeNgCard()
        {
            this.SizeChanged += NgCard_SizeChanged;
        }

        // 다른 생성자
        public NgCard(ImageSource imageSource)
        {
            InitializeComponent();
            InitializeNgCard();
            _api = new MSDApi();
            SelectedImage.Source = imageSource;
        }

        // XAML에서 Loaded="NgCard_Loaded"
        private async void NgCard_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("[NgCard_Loaded 호출됨] 윈도우가 실제로 로드되었습니다.");

            try
            {
                // 필요시 로그인
                // bool loginOk = await _api.LoginAsync("사용자ID", "암호");
                // if (!loginOk) { MessageBox.Show("로그인 실패"); return; }

                var lineIds = new List<string> { "vp01" };
                int offset = 2;
                int count = 10;

                Console.WriteLine($"[NgCard_Loaded] GetNgImagesAsync 호출: lineIds={string.Join(",", lineIds)}, offset={offset}, count={count}");

                var ngImages = await _api.GetNgImagesAsync(lineIds, offset, count);
                if (ngImages == null)
                {
                    MessageBox.Show("NG 이미지 조회 실패");
                    return;
                }

                // 전체 목록을 캐싱
                _cachedList = ngImages;

                // ListView에 DTO 전체를 바인딩
                // (Id, NgImgPath, etc.가 들어있는 VisionNgDTO)
                ImageListView.ItemsSource = _cachedList;

                // 디스플레이 편의를 위해 DisplayMemberPath나 DataTemplate 활용
                // 여기선 XAML에서 DisplayMemberPath="Id" 등으로 설정했다고 가정
                Console.WriteLine($"[NgCard_Loaded] 받아온 이미지 개수: {_cachedList.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}");
            }
        }

        // ListView에서 항목 클릭 시 -> ID별로 상세조회 -> base64 로드
        private async void ImageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show("[ImageListView_SelectionChanged] 항목 클릭됨.");

            // SelectedItem이 VisionNgDTO라 가정
            if (ImageListView.SelectedItem is VisionNgDTO selectedDto)
            {
                try
                {
                    // (1) id만 뽑아서 재조회
                    var detailList = await _api.GetNgImagesByIdsAsync(new List<int> { selectedDto.Id });
                    if (detailList == null || detailList.Count == 0)
                    {
                        MessageBox.Show("상세 조회 결과 없음");
                        return;
                    }

                    // 첫 번째 결과의 base64
                    var detailDto = detailList[0];
                    if (!string.IsNullOrEmpty(detailDto.NgImgBase64))
                    {
                        // (2) base64 -> BitmapImage
                        var bmp = Base64ImageHelper.ConvertBase64ToBitmapImage(detailDto.NgImgBase64);
                        if (bmp != null)
                        {
                            SelectedImage.Source = bmp;
                        }
                        else
                        {
                            MessageBox.Show("Base64 디코딩 실패");
                        }
                    }
                    else
                    {
                        MessageBox.Show("ngImgBase64가 null입니다");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지 로드 오류: {ex.Message}");
                }
            }
        }
    }
}
