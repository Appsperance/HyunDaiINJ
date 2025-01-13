using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using HyunDaiINJ.Services;
using HyunDaiINJ.ViewModels.Main;

namespace HyunDaiINJ.ViewModels.Login
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private readonly MSDApi _api;

        public LoginViewModel()
        {
            _api = new MSDApi();
            LoginCommand = new RelayCommand(async () => await OnLoginAsync());
        }

        /// <summary>
        /// 사용자 입력: 아이디
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 사용자 입력: 비밀번호
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 로그인 버튼을 클릭했을 때 실행할 명령
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// 실제 로그인 로직 (API 호출)
        /// </summary>
        private async Task OnLoginAsync()
        {
            // API 호출
            bool result = await _api.LoginAsync(Username, Password);

            if (result)
            {
                // 로그인 성공
                // Console에 성공 메시지 출력은 이미 MSDApi 내에서 처리 중
                // 추가로, 윈도우 전환/화면 이동 등 수행 가능
            }
            else
            {
                // 로그인 실패
                // 실패 원인은 이미 Console 출력
            }
        }

        #region INotifyPropertyChanged 구현
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
