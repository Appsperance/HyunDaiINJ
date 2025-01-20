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
            // (1) 기본값 설정
            Username = "admin1";
            Password = "string";
        }
        private string _loggedInName;
        public string LoggedInName
        {
            get => _loggedInName;
            set
            {
                if (_loggedInName != value)
                {
                    _loggedInName = value;
                    OnPropertyChanged();
                }
            }
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
            var loginResult = await _api.LoginAsync(Username, Password);

            if (loginResult == null)
            {
                MessageBox.Show("로그인 실패", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 로그인 성공
            // role 검사
            var role = JwtParser.ExtractRoleFromJwt(MSDApi.JwtToken);

            if (role == "systemAdmin" || Regex.IsMatch(role, @"^admin.*$", RegexOptions.IgnoreCase))
            {
                // 서버 응답의 name 필드를 LoginViewModel에 저장
                LoggedInName = loginResult.Name;

                // 이벤트 호출로 View에게 알려줌
                LoginSuccess?.Invoke();

                Console.WriteLine($"[로그인 성공] Name={loginResult.Name}, EmployeeNumber={loginResult.EmployeeNumber}");
            }
            else
            {
                MessageBox.Show("권한 부족", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
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
