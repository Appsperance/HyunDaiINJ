using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HyunDaiINJ.Views.Monitoring.Pages.Monitoring
{
    public partial class ProcessMonitoring : Page
    {
        private readonly DispatcherTimer _timer;

        public ProcessMonitoring()
        {
            InitializeComponent();

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
    }
}
