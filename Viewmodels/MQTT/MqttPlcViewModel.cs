using System.ComponentModel;
using System.Runtime.CompilerServices;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.ViewModels.MQTT
{
    public class MqttPlcViewModel : INotifyPropertyChanged
    {
        private string? x20;
        public string? X20
        {
            get => x20;
            set
            {
                x20 = value;
                OnPropertyChanged();
            }
        }

        private string? x21;
        public string? X21
        {
            get => x21;
            set
            {
                x21 = value;
                OnPropertyChanged();
            }
        }

        private string? y40;
        public string? Y40
        {
            get => y40;
            set
            {
                y40 = value;
                OnPropertyChanged();
            }
        }

        // ... 이하 D1, D2, Y41, Y42, Y43도 동일한 패턴으로 구현

        private string? d1;
        public string? D1
        {
            get => d1;
            set
            {
                d1 = value;
                OnPropertyChanged();
            }
        }

        private string? d2;
        public string? D2
        {
            get => d2;
            set
            {
                d2 = value;
                OnPropertyChanged();
            }
        }

        private string? y41;
        public string? Y41
        {
            get => y41;
            set
            {
                y41 = value;
                OnPropertyChanged();
            }
        }

        private string? y42;
        public string? Y42
        {
            get => y42;
            set
            {
                y42 = value;
                OnPropertyChanged();
            }
        }

        private string? y43;
        public string? Y43
        {
            get => y43;
            set
            {
                y43 = value;
                OnPropertyChanged();
            }
        }
        private bool isInputBlinking;
        public bool IsInputBlinking
        {
            get => isInputBlinking;
            set
            {
                isInputBlinking = value;
                OnPropertyChanged();
            }
        }

        private bool isVisionBlinking;
        public bool IsVisionBlinking
        {
            get => isVisionBlinking;
            set
            {
                isVisionBlinking = value;
                OnPropertyChanged();
            }
        }

        private bool isCompleteBlinking;
        public bool IsCompleteBlinking
        {
            get => isCompleteBlinking;
            set
            {
                isCompleteBlinking = value;
                OnPropertyChanged();
            }
        }

        //===========================================================
        // INotifyPropertyChanged 구현
        //===========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
