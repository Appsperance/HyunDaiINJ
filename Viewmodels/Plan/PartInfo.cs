using System.ComponentModel;

namespace HyunDaiINJ.Models.Plan
{
    public class PartInfo : INotifyPropertyChanged
    {
        private static int s_autoIncrementId = 1;

        // 고유한 PartId (자동 증가)
        private int _partId;
        public int PartId
        {
            get => _partId;
            set
            {
                _partId = value;
                OnPropertyChanged(nameof(PartId));
            }
        }

        // 사용자가 원하는 대로 변경 가능한 이름 (헤더 표시용)
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public PartInfo()
        {
            PartId = s_autoIncrementId++;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}