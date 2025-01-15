using HyunDaiINJ.ViewModels.Login;
using HyunDaiINJ.ViewModels.Main;
using System;
using System.Windows;
using System.Windows.Input;

namespace HyunDaiINJ.Views.Login
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            // ▼ 여기 추가: DataContext가 LoginViewModel인지 확인 후, LoginSuccess 이벤트 구독
            if (DataContext is LoginViewModel vm)
            {
                vm.LoginSuccess += OnLoginSuccess;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = txtPassword.Password;
                vm.LoginCommand.Execute(null);
            }
        }

        /// <summary>
        /// ViewModel에서 LoginSuccess 이벤트가 발생하면 호출됨
        /// </summary>
        private void OnLoginSuccess()
        {
            // MainView 열기
            var mainView = new MainView();
            // (2) MainViewModel 생성 후 할당
            mainView.DataContext = new MainViewModel();
            

            mainView.Show();

            Console.WriteLine($"mainViewVMVMVMVMVM : {mainView.DataContext}");
            // 현재(LoginView) 창 닫기
            this.Close();
        }
    }
}
