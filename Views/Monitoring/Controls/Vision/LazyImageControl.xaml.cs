using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HyunDaiINJ.Views.Monitoring.Controls.Vision
{
    public partial class LazyImageControl : UserControl
    {
        // (1) 실제 이미지 경로를 바인딩받을 DependencyProperty
        public static readonly DependencyProperty SourcePathProperty =
            DependencyProperty.Register(
                nameof(SourcePath),
                typeof(string),
                typeof(LazyImageControl),
                new PropertyMetadata(null));

        public string SourcePath
        {
            get => (string)GetValue(SourcePathProperty);
            set => SetValue(SourcePathProperty, value);
        }

        // (2) 로드 여부
        private bool _isImageLoaded = false;

        public LazyImageControl()
        {
            InitializeComponent();
        }

        // (3) 화면에 나타나는 순간(또는 IsVisible 바뀌는 순간) 실제 로딩
        private void RootGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // 보이는 상태가 되었고, 아직 로드 안했으면 로드
            if (e.NewValue is bool isVisible && isVisible && !_isImageLoaded)
            {
                LoadImage();
            }
        }

        private void LoadImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(SourcePath))
                {
                    // 실제 BitmapImage 로드
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(SourcePath, UriKind.Absolute);
                    // 필요 시 CacheOption / CreateOptions 등 설정
                    bmp.CacheOption = BitmapCacheOption.OnDemand;
                    bmp.CreateOptions = BitmapCreateOptions.DelayCreation;
                    bmp.EndInit();

                    // 이미지 표시
                    LazyImage.Source = bmp;
                    LazyImage.Visibility = Visibility.Visible;

                    _isImageLoaded = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LazyImageControl Load 실패: {ex.Message}");
                // 에러 이미지 대체 등 처리
            }
        }
    }
}
