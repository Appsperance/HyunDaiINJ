using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.MQTT;

namespace HyunDaiINJ.Views.MQTT
{
    public partial class RealTimeMonitoring : Page
    {
        public RealTimeMonitoring()
        {
            InitializeComponent();

            var mqttModel = new MQTTModel();

            var mqttViewModel = new MqttViewModel(mqttModel);

            DataContext = mqttViewModel;

        }
    }
}
