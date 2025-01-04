using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HyunDaiINJ.DTO;
using Newtonsoft.Json;
using WpfApp1;

namespace HyunDaiINJ.ViewModels.MQTT
{
    public class MqttViewModel : INotifyPropertyChanged
    {
        private readonly MQTTModel MqttModel; // Model 인스턴스 (의존성 주입)
        // MQTT 연결 상태
        public bool mqttConnected => MqttModel.mqttConnected;

        // 수신된 메시지 리스트
        public ObservableCollection<MQTTDTO> ReceivedMessage { get; } = new ObservableCollection<MQTTDTO>();


        // 최신 메시지
        private MQTTDTO currentMessage;
        public MQTTDTO CurrentMessage
        {
            get => currentMessage;
            set
            {
                currentMessage = value;
                OnPropertyChanged();
            }
        }
        // 현재 표시 중인 이미지
        private BitmapImage currentImage;
        public BitmapImage CurrentImage
        {
            get => currentImage;
            set
            {
                currentImage = value;
                OnPropertyChanged();
            }
        }

        // 기본 생성자
        public MqttViewModel() : this(new MQTTModel())
        {
        }

        public MqttViewModel(MQTTModel mqttModel)
        {
            MqttModel = mqttModel ?? throw new ArgumentNullException(nameof(mqttModel));

            // MQTT 연결 및 구독
            ConnectAndSubscribeAsync().ConfigureAwait(false);

            // 메시지 수신 이벤트 구독
            MqttModel.MqttMesageReceived += OnMqttMessageReceived;
        }

        private async Task ConnectAndSubscribeAsync()
        {
            try
            {
                await MqttModel.MqttConnect(); // MQTT 연결
                if (mqttConnected)
                {
                    Console.WriteLine("MQTT 자동 연결 성공");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MQTT 자동 연결 실패: {ex.Message}");
            }
        }

        private void OnMqttMessageReceived(string topic, MQTTDTO message)
        {
            // UI 스레드에서 작업
            App.Current.Dispatcher.Invoke(() =>
            {
                CurrentMessage = message; // 최신 메시지 업데이트
                Console.WriteLine($"수신된 토픽: {topic}, 메시지: {message}");

                // 이미지 데이터 처리
                if (message.NgImg != null && message.NgImg.Length > 0)
                {
                    CurrentImage = ConvertImage(message.NgImg); // 이미지 변환 및 설정
                    Console.WriteLine("이미지 데이터 처리 완료");
                }
            });
        }

        // 이미지 변환 (byte[] -> BitmapImage)
        private BitmapImage ConvertImage(byte[] image)
        {
            try
            {
                using (var ms = new MemoryStream(image))
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
