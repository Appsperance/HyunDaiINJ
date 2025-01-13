using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using HyunDaiINJ.DATA.DTO;
using HyunDaiINJ.ViewModels.MQTT;

namespace HyunDaiINJ.Views.Monitoring.Pages.Monitoring
{
    public partial class ProcessMonitoring : Page
    {
        // MQTT 메시지 처리
        private MQTTModel _mqttModel;
        // 실제로 UI에 바인딩할 ViewModel
        private MqttPlcViewModel _plcVM;
        private DispatcherTimer _timer;

        public ProcessMonitoring()
        {
            InitializeComponent();

            _plcVM = new MqttPlcViewModel();
            this.DataContext = _plcVM;   // ★ DataContext 설정 ★

            _mqttModel = new MQTTModel();

            // MQTT 연결 및 구독 진행 (비동기로 실행)
            _ = ConnectAndSubscribe();

            // 타이머 초기화
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // 1초마다 갱신
            _timer.Tick += (s, e) =>
            {
                // 매 Tick마다 현재 시간을 표시
                TxtCurrentTime.Text = DateTime.Now.ToString("yyyy-MM-dd (ddd) HH시 mm분 ss초");
            };

            // 타이머 시작
            _timer.Start();
        }

        private async Task ConnectAndSubscribe()
        {
            await _mqttModel.MqttConnect();
            await _mqttModel.SubscribeMQTT("Process/PLC");

            // 이벤트 구독
            _mqttModel.ProcessMessageReceived += (topic, dto) =>
            {
                // UI 스레드에서 ViewModel 업데이트
                Dispatcher.Invoke(() =>
                {
                    // dto에 들어온 값 -> ViewModel에 대입
                    _plcVM.X20 = dto.X20;
                    _plcVM.X21 = dto.X21;
                    _plcVM.Y40 = dto.Y40;
                    _plcVM.D1 = dto.D1;
                    _plcVM.D2 = dto.D2;
                    // 예: DTO는 "Y41":"1" 형태로 오고, 
                    // 시퀀스 시작하려면 _plcVM.Y41 = "Y41" 처럼 "문자열"로 넣어주는 식
                    if (dto.Y41 == "1")
                    {
                        // 여기선 "Y41"이라는 문자열 자체를 넣어야 
                        // UserControl에서 분기(Y41 case)로 인식
                        _plcVM.Y41 = "Y41";
                    }
                    else if (dto.Y42 == "1")
                    {
                        _plcVM.Y41 = "Y42";
                    }
                    else if (dto.Y43 == "1")
                    {
                        _plcVM.Y41 = "Y43";
                    }
                    else
                    {
                        // 아무것도 아닌 경우
                        _plcVM.Y41 = "";
                    }
                });
            };
        }


    }
}
