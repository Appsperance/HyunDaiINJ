using HyunDaiINJ.ViewModels;
using HyunDaiINJ.ViewModels.Main;
using HyunDaiINJ.Views.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HyunDaiINJ.Views
{
    /// <summary>
    /// MainView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainView : Window
    {

        private DispatcherTimer _timer;

        public MainView()
        {
            InitializeComponent();
            //this.DataContext = new MainViewModel();

            // 만약 '현재시간'만 표시하면 되므로, 굳이 DataContext는 안 써도 됨.
            // _timer 설정
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) =>
            {
                TxtCurrentTime.Text = DateTime.Now.ToString("yyyy-MM-dd (ddd) HH시 mm분 ss초");
            };
            _timer.Start();
        }

        private void Logout_btn_Click(object sender, RoutedEventArgs e)
        {
            // 1) LoginView 다시 열기
            var loginView = new LoginView();
            loginView.Show();

            // 2) 현재 MainView 닫기
            this.Close();
        }
    }
}
