using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HyunDaiINJ.ViewModels.Monitoring.Process
{
    public class ProcessProcedureViewModel : INotifyPropertyChanged
    {
        // 필드
        private int _sequenceToken = 0;

        private string? stageVal;
        public string? StageVal
        {
            get => stageVal;
            set
            {
                stageVal = value;
                OnPropertyChanged();
                OnStageValUpdated(stageVal);
            }
        }

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

        private bool _isInputType;
        public bool IsInputType
        {
            get => _isInputType;
            set
            {
                if (_isInputType != value)
                {
                    _isInputType = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _heatingType;
        public bool HeatingType
        {
            get => _heatingType;
            set
            {
                if (_heatingType != value)
                {
                    _heatingType = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _takeOutStyle;
        public bool TakeOutStyle
        {
            get => _takeOutStyle;
            set
            {
                if (_takeOutStyle != value)
                {
                    _takeOutStyle = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private async void OnStageValUpdated(string? newVal)
        {
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
                        "Resources/1.png","Resources/2.png","Resources/3.png","Resources/4.png",
                        "Resources/5.png","Resources/6.png","Resources/7.png","Resources/8.png",
                        "Resources/9.png","Resources/10.png","Resources/11.png","Resources/12.png",
                        "Resources/13.png","Resources/14.png"
                    },localToken);
                    break;
                case "Y42":
                    IsInputType = false;
                    HeatingType = true;
                    TakeOutStyle = false;
                    await DisplayImageSequenceAsync(new[]
                    {
                        "Resources/15.png","Resources/16.png","Resources/17.png","Resources/18.png",
                        "Resources/19.png","Resources/20.png","Resources/21.png","Resources/22.png"
                    }, localToken);
                    break;
                case "Y43":
                    IsInputType = false;
                    HeatingType = false;
                    TakeOutStyle = true;
                    await DisplayImageSequenceAsync(new[]
                    {
                        "Resources/23.png","Resources/24.png","Resources/25.png","Resources/26.png",
                        "Resources/27.png","Resources/28.png","Resources/29.png"
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
                {// ★ 토큰 체크
                 //   현재 OnStageValUpdated(...)에서 _sequenceToken이 또 바뀌면
                 //   여기서 token != _sequenceToken => 이전 시퀀스 중단
                if (token != _sequenceToken)
                {
                    // 시퀀스 도중에 새 StageVal이 들어옴 => 중단
                    return;
                }
                try
                {
                    // ★ 수정안
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var fullPath = Path.Combine(baseDir, path); // path = "Resources/1.png"
                    System.Diagnostics.Debug.WriteLine(fullPath);

                    var bitmap = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
                    
                    CurrentImage = bitmap;
                    Console.WriteLine(CurrentImage);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcessProcedureViewModel] 로드 실패: {path}, {ex.Message}");
                }
                await Task.Delay(500);
            }
        }
    }
}
