using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
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
        /// 로그인 성공 시 View에 알려줄 이벤트
        /// </summary>
        public event Action LoginSuccess;

        /// <summary>
        /// 로그인 버튼을 클릭했을 때 실행할 명령
        /// </summary>
        public ICommand LoginCommand { get; }

        private async Task OnLoginAsync()
        {
            // 로그인 API 호출
            bool isLoginSuccess = await _api.LoginAsync(Username, Password);

            if (isLoginSuccess)
            {
                // role 추출
                var role = JwtParser.ExtractRoleFromJwt(MSDApi.JwtToken);

                if (role == "systemAdmin" || Regex.IsMatch(role, @"^admin\d*$", RegexOptions.IgnoreCase))
                {
                    // LoginSuccess 이벤트 발생
                    LoginSuccess?.Invoke();
                }
                else
                {
                    // 권한이 맞지 않을 때
                    System.Windows.MessageBox.Show("권한 부족", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                // 로그인 실패
                System.Windows.MessageBox.Show("로그인 실패", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
