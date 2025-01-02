using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HyunDaiINJ.Dto;
using Newtonsoft.Json;

namespace HyunDaiINJ.ViewModels.MQTT
{
    public class MqttViewModel : INotifyPropertyChanged
    {
        private readonly MqttService _mqttService;
        private RabbitMQQualityOneDTO _qualityOneMessage;
        private BitmapImage _qualityOneImage;
        private bool _isConnected;

        public RabbitMQQualityOneDTO QualityOneMessage
        {
            get => _qualityOneMessage;
            set
            {
                _qualityOneMessage = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage QualityOneImage
        {
            get => _qualityOneImage;
            set
            {
                _qualityOneImage = value;
                Console.WriteLine("QualityOneImage 업데이트됨");
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        public MqttViewModel()
        {
            _mqttService = new MqttService();
            _mqttService.OnQualityOneMessageReceived += HandleQualityOneMessageReceived;
        }

        public async Task ConnectAndSubscribeAsync(string brokerAddress, string username, string password)
        {
            await _mqttService.ConnectAsync(brokerAddress, username, password);
            IsConnected = true;

            await _mqttService.SubscribeAsync("Quality/one/#");
        }

       
        private void HandleQualityOneMessageReceived(string topic, RabbitMQQualityOneDTO message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    QualityOneMessage = message;

                    // 디버깅: 메시지 로그 출력
                    Console.WriteLine($"수신된 메시지: {JsonConvert.SerializeObject(message)}");

                    // 디버깅: Base64 문자열 시작 및 끝 확인
                    if (!string.IsNullOrEmpty(message.ProductImage))
                    {
                        Console.WriteLine($"Base64 이미지 데이터 길이: {message.ProductImage.Length}");
                        Console.WriteLine($"Base64 시작: {message.ProductImage.Substring(0, Math.Min(50, message.ProductImage.Length))}...");
                        Console.WriteLine($"Base64 끝: ...{message.ProductImage.Substring(Math.Max(0, message.ProductImage.Length - 50))}");

                        QualityOneImage = ConvertBase64ToBitmapImage(message.ProductImage);
                        Console.WriteLine($"QualityOneImage 업데이트: {QualityOneImage != null}");

                    }
                    else
                    {
                        Console.WriteLine("수신된 메시지에 이미지가 포함되지 않았습니다.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"MQTT 메시지 처리 중 오류: {ex.Message}");
                }
            });
        }

        private BitmapImage ConvertBase64ToBitmapImage(string base64Image)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Image))
                {
                    throw new Exception("Base64 문자열이 비어 있습니다.");
                }

                // Base64 문자열을 바이트 배열로 변환
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                Console.WriteLine($"Base64 이미지 바이트 배열 길이: {imageBytes.Length}");

                // MemoryStream을 사용해 BitmapImage 생성
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"이미지 변환 실패: {ex.Message}");
                return null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
