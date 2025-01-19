using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HyunDaiINJ.DATA.DTO;
using Newtonsoft.Json;
using WpfApp1;

namespace HyunDaiINJ.ViewModels.MQTT
{
    public class MqttVisionViewModel : INotifyPropertyChanged
    {
        private readonly MQTTModel _mqttModel;

        // MQTT 연결 상태
        public bool MqttConnected => _mqttModel.MqttConnected;

        // 최신 메시지
        private MqttVisionDTO _currentMessage;
        public MqttVisionDTO CurrentMessage
        {
            get => _currentMessage;
            set
            {
                _currentMessage = value;
                OnPropertyChanged();
            }
        }

        // 현재 표시 중인 이미지
        private BitmapImage? _currentImage;
        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                OnPropertyChanged();
            }
        }

        // 단계별 이미지
        private BitmapImage? _stageValImage;
        public BitmapImage? StageValImage
        {
            get => _stageValImage;
            set
            {
                _stageValImage = value;
                OnPropertyChanged();
            }
        }

        // 깜빡임 상태
        private bool _isInputBlinking;
        public bool IsInputBlinking
        {
            get => _isInputBlinking;
            set
            {
                _isInputBlinking = value;
                OnPropertyChanged();
            }
        }

        private bool _isVisionBlinking;
        public bool IsVisionBlinking
        {
            get => _isVisionBlinking;
            set
            {
                _isVisionBlinking = value;
                OnPropertyChanged();
            }
        }

        private bool _isCompleteBlinking;
        public bool IsCompleteBlinking
        {
            get => _isCompleteBlinking;
            set
            {
                _isCompleteBlinking = value;
                OnPropertyChanged();
            }
        }

        // 품질 이미지를 저장
        private ObservableCollection<BitmapImage> _qualityImages = new();
        public ObservableCollection<BitmapImage> QualityImages
        {
            get => _qualityImages;
            set
            {
                _qualityImages = value;
                OnPropertyChanged();
            }
        }

        public MqttVisionViewModel(MQTTModel mqttModel)
        {
            _mqttModel = mqttModel ?? throw new ArgumentNullException(nameof(mqttModel));

            // 필드 초기화
            _currentMessage = new MqttVisionDTO();
            _currentImage = new BitmapImage();

            // MQTT 연결 및 구독
            ConnectAndSubscribeAsync().ConfigureAwait(false);

            // 메시지 수신 이벤트 구독
            _mqttModel.VisionMessageReceived += OnVisionMessageReceived;
        }

        private async Task ConnectAndSubscribeAsync()
        {
            try
            {
                await _mqttModel.MqttConnect(); // MQTT 연결

                if (MqttConnected)
                {
                    Console.WriteLine("MQTT 연결 성공");
                    await _mqttModel.SubscribeMQTT("Vision/ng/#"); // Vision 관련 토픽 구독
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MQTT 연결 실패: {ex.Message}");
            }
        }

        private async void OnVisionMessageReceived(string topic, MqttVisionDTO message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                CurrentMessage = message;
                UpdateBlinkingStates(message.StageVal);

                if (message.NgImg != null && message.NgImg.Length > 0)
                {
                    var image = ConvertImage(message.NgImg);
                    CurrentImage = image;
                    QualityImages.Add(image);
                }
            });

            await HandleStageValImagesAsync(message.StageVal);
        }

        private async Task HandleStageValImagesAsync(string stageVal)
        {
            string[] imagePaths = stageVal switch
            {
                "100" => new[]
                {
                    "Resources/1.png", "Resources/2.png", "Resources/3.png", "Resources/4.png",
                    "Resources/5.png", "Resources/6.png", "Resources/7.png", "Resources/8.png",
                    "Resources/9.png", "Resources/10.png", "Resources/11.png", "Resources/12.png",
                    "Resources/13.png", "Resources/14.png"
                },
                "010" => new[]
                {
                    "Resources/15.png", "Resources/16.png", "Resources/17.png", "Resources/18.png",
                    "Resources/19.png", "Resources/20.png", "Resources/21.png", "Resources/22.png"
                },
                "001" => new[]
                {
                    "Resources/23.png", "Resources/24.png", "Resources/25.png", "Resources/26.png",
                    "Resources/27.png", "Resources/28.png", "Resources/29.png"
                },
                _ => Array.Empty<string>()
            };

            if (imagePaths.Length > 0)
            {
                await DisplayImageSequenceAsync(imagePaths);
            }
        }

        private async Task DisplayImageSequenceAsync(string[] imagePaths)
        {
            foreach (var path in imagePaths)
            {
                var image = LoadImageFromPath(path);
                if (image != null)
                {
                    App.Current.Dispatcher.Invoke(() => StageValImage = image);
                    await Task.Delay(300); // 각 이미지 표시 시간
                }
            }
        }

        private BitmapImage? LoadImageFromPath(string relativePath)
        {
            try
            {
                var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"이미지 로드 실패: {ex.Message}");
                return null;
            }
        }

        private BitmapImage? ConvertImage(byte[] image)
        {
            try
            {
                using var ms = new MemoryStream(image);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"이미지 변환 실패: {ex.Message}");
                return null;
            }
        }

        private void UpdateBlinkingStates(string stageVal)
        {
            IsInputBlinking = stageVal == "100";
            IsVisionBlinking = stageVal == "010";
            IsCompleteBlinking = stageVal == "001";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
