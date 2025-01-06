﻿using System;
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
    public class MqttViewModel : INotifyPropertyChanged
    {
        private readonly MQTTModel MqttModel; // Model 인스턴스 (의존성 주입)
        // MQTT 연결 상태
        public bool mqttConnected => MqttModel.mqttConnected;

        // 최신 메시지
        private MqttVisionDTO currentMessage;
        public MqttVisionDTO CurrentMessage
        {
            get => currentMessage;
            set
            {
                currentMessage = value;
                OnPropertyChanged();
            }
        }
        // 현재 표시 중인 이미지
        private BitmapImage? currentImage;
        public BitmapImage? CurrentImage
        {
            get => currentImage;
            set
            {
                currentImage = value;
                OnPropertyChanged();
            }
        }

        // 최신 메시지
        private MqttProcessDTO currentProcessMessage;
        public MqttProcessDTO CurrentProcessMessage
        {
            get => currentProcessMessage;
            set
            {
                currentProcessMessage = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage? stageValImage;
        public BitmapImage? StageValImage
        {
            get => stageValImage;
            set
            {
                stageValImage = value;
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

            // 필드 초기화
            currentMessage = new MqttVisionDTO();
            currentImage = new BitmapImage();

            // MQTT 연결 및 구독
            ConnectAndSubscribeAsync().ConfigureAwait(false);

            // 메시지 수신 이벤트 구독
            MqttModel.VisionMessageReceived += OnVisionMessageReceived;
            MqttModel.ProcessMessageReceived += OnProcessMessageReceived;

        }

        private async Task ConnectAndSubscribeAsync()
        {
            try
            {
                await MqttModel.MqttConnect(); // MQTT 연결

                if (mqttConnected) // 연결 성공 여부 확인
                {
                    Console.WriteLine("MQTT 자동 연결 성공"); // 성공 메시지 출력
                    await MqttModel.SubscribeMQTT("#"); // 토픽 구독
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MQTT 자동 연결 실패: {ex.Message}");
            }
        }

        private async void OnVisionMessageReceived(string topic, MqttVisionDTO message)
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

            switch (message.StageVal)
            {
                case "100":
                    await DisplayImageSequenceAsync(new[] { "Resources/Process1.png", "Resources/Process2.png" });
                    break;
                case "010":
                    await DisplayImageSequenceAsync(new[] { "Resources/Process3.png", "Resources/Process4.png" });
                    break;
                case "001":
                    await DisplayImageSequenceAsync(new[] { "Resources/Process5.png", "Resources/Process6.png" });
                    break;
                default:
                    App.Current.Dispatcher.Invoke(() => StageValImage = null); // 기본 상태
                    break;
            }
        }

        private async Task DisplayImageSequenceAsync(string[] imagePaths)
        {
            foreach (var path in imagePaths)
            {
                var image = LoadImageFromPath(path);

                // UI 스레드에서 StageValImage 업데이트
                App.Current.Dispatcher.Invoke(() => StageValImage = image);

                // 1초 대기
                await Task.Delay(1000);
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

        // 이미지 변환 (byte[] -> BitmapImage)
        private static BitmapImage? ConvertImage(byte[] image)
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

        private void OnProcessMessageReceived(string topic, MqttProcessDTO message) =>
            App.Current.Dispatcher.Invoke(() =>
            {
                Console.WriteLine($"수신된 Process 토픽: {topic}, 메시지: {message}");

                // Process 데이터를 처리하는 로직 추가
                // 예: UI 업데이트 또는 데이터 저장
            });


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}