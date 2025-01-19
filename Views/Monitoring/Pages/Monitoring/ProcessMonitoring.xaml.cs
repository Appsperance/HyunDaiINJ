using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.ViewModels.Monitoring.Process;

namespace HyunDaiINJ.Views.Monitoring.Pages.Monitoring
{
    public partial class ProcessMonitoring : Page
    {
        private readonly MQTTModel _mqttModel;
        private readonly ProcessProcedureViewModel _plcVM;
        private readonly DispatcherTimer _timer;

        public ProcessMonitoring()
        {
            InitializeComponent();

            // MQTTModel 생성
            _mqttModel = new MQTTModel();

            // MqttPlcViewModel 생성 및 MQTTModel 연결
            _plcVM = new ProcessProcedureViewModel(_mqttModel);
            this.DataContext = _plcVM;

            // MQTT 연결 및 구독
            _ = ConnectAndSubscribe();

            // 타이머 설정
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

        private async Task ConnectAndSubscribe()
        {
            await _mqttModel.MqttConnect();
            await _mqttModel.SubscribeMQTT("Process/PLC/#");
        }
    }
}
