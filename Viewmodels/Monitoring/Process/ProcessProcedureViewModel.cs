using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.ViewModels.Monitoring.Process
{
    public class ProcessProcedureViewModel : INotifyPropertyChanged
    {
        private readonly MQTTModel _mqttModel;
        private int _sequenceToken = 0;

        public ProcessProcedureViewModel(MQTTModel mqttModel)
        {
            _mqttModel = mqttModel ?? throw new ArgumentNullException(nameof(mqttModel));
            _mqttModel.ProcessMessageReceived += OnProcessMessageReceived;
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

        private BitmapImage? _currentImage;
        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }


        // MQTT 메시지 수신 처리
        private void OnProcessMessageReceived(string topic, MqttProcessDTO message)
        {
            X20 = message.X20;
            X21 = message.X21;
            Y40 = message.Y40;
            D1 = message.D1;
            D2 = message.D2;

            // StageVal 업데이트
            if (message.Y41 == "1") StageVal = "Y41";
            else if (message.Y42 == "1") StageVal = "Y42";
            else if (message.Y43 == "1") StageVal = "Y43";
            else StageVal = null;

            Console.WriteLine($"[ProcessProcedureViewModel] 데이터 업데이트 - X20: {X20}, Y40: {Y40}, StageVal: {StageVal}");
        }

        // StageVal 변경 시 이미지와 상태 업데이트
        private async void OnStageValUpdated(string? newVal)
        {
            Console.WriteLine($"[ProcessProcedureViewModel] OnStageValUpdated 호출됨 - newVal: {newVal}");
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
                        "Resources/5.png", "Resources/6.png", "Resources/7.png", "Resources/8.png"
                    }, localToken);
                    break;

                case "Y42":
                    IsInputType = false;
                    HeatingType = true;
                    TakeOutStyle = false;
                    await DisplayImageSequenceAsync(new[]
                    {
                        "Resources/15.png", "Resources/16.png", "Resources/17.png", "Resources/18.png"
                    }, localToken);
                    break;

                case "Y43":
                    IsInputType = false;
                    HeatingType = false;
                    TakeOutStyle = true;
                    await DisplayImageSequenceAsync(new[]
                    {
                        "Resources/23.png", "Resources/24.png", "Resources/25.png", "Resources/26.png"
                    }, localToken);
                    break;

                default:
                    CurrentImage = null;
                    IsInputType = false;
                    HeatingType = false;
                    TakeOutStyle = false;
                    break;
            }
        }

        private async Task DisplayImageSequenceAsync(IEnumerable<string> imagePaths, int token)
        {
            foreach (var path in imagePaths)
            {
                if (token != _sequenceToken) return;

                try
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var fullPath = Path.Combine(baseDir, path);

                    Console.WriteLine($"[DisplayImageSequenceAsync] 이미지 경로: {fullPath}, 파일 존재 여부: {File.Exists(fullPath)}");

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    CurrentImage = bitmap;

                    Debug.WriteLine($"[DisplayImageSequenceAsync] 이미지 로드 성공: {fullPath}");
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DisplayImageSequenceAsync] 이미지 로드 실패: {path}, {ex.Message}");
                }
            }
        }


        // INotifyPropertyChanged 구현
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
