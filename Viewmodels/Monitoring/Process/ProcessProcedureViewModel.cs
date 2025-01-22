using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HyunDaiINJ.DATA.DTO;
using WpfApp1;

namespace HyunDaiINJ.ViewModels.Monitoring.Process
{
    public class ProcessProcedureViewModel : INotifyPropertyChanged
    {
        private readonly MQTTModel _mqttModel;

        // 이 뷰모델이 구독할 토픽 (ex: "Process/PLC/1")
        private readonly string _processTopic;

        // 이미지 시퀀스가 바뀔 때 중단제어 용도 (여러 Stage가 겹치지 않게)
        private int _sequenceToken = 0;

        public ProcessProcedureViewModel(MQTTModel mqttModel, string processTopic)
        {
            _mqttModel = mqttModel ?? throw new ArgumentNullException(nameof(mqttModel));
            _processTopic = processTopic;

            // (1) 기본 이미지 초기화
            CurrentImage = LoadImage("Resources/30.png");
            // MQTT 메시지 수신 핸들러 등록
            _mqttModel.ProcessMessageReceived += OnProcessMessageReceived;

            // MQTT 연결 및 해당 토픽 구독
            _ = ConnectAndSubscribe();
        }
        private BitmapImage? LoadImage(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadImage 실패: {ex.Message}");
                return null;
            }
        }
        private async Task ConnectAndSubscribe()
        {
            await _mqttModel.MqttConnect();
            await _mqttModel.SubscribeMQTT(_processTopic);
        }

        // PLC 데이터 속성
        private string? _x20;
        public string? X20
        {
            get => _x20;
            set => SetProperty(ref _x20, value);
        }

        private string? _x21;
        public string? X21
        {
            get => _x21;
            set => SetProperty(ref _x21, value);
        }

        private string? _y40;
        public string? Y40
        {
            get => _y40;
            set => SetProperty(ref _y40, value);
        }

        private string? _d1;
        public string? D1
        {
            get => _d1;
            set => SetProperty(ref _d1, value);
        }

        private string? _d2;
        public string? D2
        {
            get => _d2;
            set => SetProperty(ref _d2, value);
        }

        private string? _stageVal;
        public string? StageVal
        {
            get => _stageVal;
            set
            {
                if (SetProperty(ref _stageVal, value))
                {
                    OnStageValUpdated(_stageVal);
                }
            }
        }

        // 상태 표시 (버튼 색상 등에 쓰일)
        private bool _isInputType;
        public bool IsInputType
        {
            get => _isInputType;
            set => SetProperty(ref _isInputType, value);
        }

        private bool _heatingType;
        public bool HeatingType
        {
            get => _heatingType;
            set => SetProperty(ref _heatingType, value);
        }

        private bool _takeOutStyle;
        public bool TakeOutStyle
        {
            get => _takeOutStyle;
            set => SetProperty(ref _takeOutStyle, value);
        }

        // 현재 표시 중인 이미지
        private BitmapImage? _currentImage;
        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        // 실제 메시지 처리
        private void OnProcessMessageReceived(string topic, MqttProcessDTO message)
        {
            // 이 뷰모델이 담당하는 토픽만 처리
            if (!topic.Equals(_processTopic, StringComparison.OrdinalIgnoreCase))
                return;

            X20 = message.X20;
            X21 = message.X21;
            Y40 = message.Y40;
            D1 = message.D1;
            D2 = message.D2;

            // StageVal 판별
            if (message.Y41 == "1") StageVal = "Y41";
            else if (message.Y42 == "1") StageVal = "Y42";
            else if (message.Y43 == "1") StageVal = "Y43";
            else StageVal = null;

            Console.WriteLine($"[ProcessProcedureViewModel {_processTopic}] " +
                              $"데이터 업데이트 - X20: {X20}, Y40: {Y40}, StageVal: {StageVal}");
        }

        // StageVal이 바뀔 때마다 이미지 애니메이션
        private async void OnStageValUpdated(string? newVal)
        {
            Console.WriteLine($"[ProcessProcedureViewModel {_processTopic}] OnStageValUpdated 호출됨 - newVal: {newVal}");

            // 새로운 단계가 시작될 때마다 sequenceToken을 증가 → 이전 시퀀스는 중단
            _sequenceToken++;
            int localToken = _sequenceToken;

            CurrentImage = null;

            switch (newVal)
            {
                case "Y41":
                    IsInputType = true;
                    HeatingType = false;
                    TakeOutStyle = false;

                    await DisplayImageSequenceAsync(new[]
                    {
                        "Resources/1.png", "Resources/2.png", "Resources/3.png", "Resources/4.png",
                        "Resources/5.png", "Resources/6.png", "Resources/7.png", "Resources/8.png",
                        "Resources/9.png", "Resources/10.png", "Resources/11.png", "Resources/12.png",
                        "Resources/13.png", "Resources/14.png"
                    }, localToken);
                    break;

                case "Y42":
                    IsInputType = false;
                    HeatingType = true;
                    TakeOutStyle = false;

                    await DisplayImageSequenceAsync(new[]
                    {
                        "Resources/15.png", "Resources/16.png", "Resources/17.png", "Resources/18.png",
                        "Resources/19.png", "Resources/20.png", "Resources/21.png", "Resources/22.png"
                    }, localToken);
                    break;

                case "Y43":
                    IsInputType = false;
                    HeatingType = false;
                    TakeOutStyle = true;

                    await DisplayImageSequenceAsync(new[]
                    {
                         "Resources/23.png", "Resources/24.png", "Resources/25.png", "Resources/26.png",
                         "Resources/27.png", "Resources/29.png"
                    }, localToken);
                    break;

                default:
                    // StageVal이 null이면 리셋
                    CurrentImage = null;
                    IsInputType = false;
                    HeatingType = false;
                    TakeOutStyle = false;
                    break;
            }

            Console.WriteLine($"[ProcessProcedureViewModel {_processTopic}] CurrentImage: {CurrentImage}");
        }

        // 0.5초 간격으로 이미지들 순차 표시
        private async Task DisplayImageSequenceAsync(IEnumerable<string> imagePaths, int token)
        {
            foreach (var path in imagePaths)
            {
                if (token != _sequenceToken) return; // Stage 변경 시 중단

                try
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var fullPath = Path.Combine(baseDir, path);

                    Console.WriteLine($"[DisplayImageSequenceAsync {_processTopic}] " +
                                      $"이미지 경로: {fullPath}, 파일 존재 여부: {System.IO.File.Exists(fullPath)}");

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    // UI 스레드에서 CurrentImage 갱신
                    App.Current.Dispatcher.Invoke(() => CurrentImage = bitmap);

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DisplayImageSequenceAsync {_processTopic}] " +
                                      $"이미지 로드 실패 - 경로: {path}, 에러: {ex.Message}");
                }
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
