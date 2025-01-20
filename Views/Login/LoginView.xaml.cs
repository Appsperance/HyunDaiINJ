using HyunDaiINJ.ViewModels.Login;
using HyunDaiINJ.ViewModels.Main;
using Prism.Events;
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
            if (DataContext is LoginViewModel loginVM)
            {
                var eventAggregator = new EventAggregator();

                var mainView = new MainView();
                var mainVM = new MainViewModel(eventAggregator);

                // 여기서 loginVM.LoggedInName을 MainViewModel.UserName 등에 세팅
                mainVM.UserName = loginVM.LoggedInName;

                mainView.DataContext = mainVM;
                mainView.Show();

                this.Close();
            }
        }
    }
}
