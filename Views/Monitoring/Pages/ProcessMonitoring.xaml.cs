using System.Windows.Controls;
using HyunDaiINJ.ViewModels.MQTT;

namespace HyunDaiINJ.Views.Monitoring.Pages
{
    /// <summary>
    /// ProcessMonitoring.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProcessMonitoring : Page
    {
        public ProcessMonitoring()
        {
            InitializeComponent();

            var mqttModel = new MQTTModel();

            var mqttViewModel = new MqttViewModel(mqttModel);

            DataContext = mqttViewModel;
        }
    }
}
