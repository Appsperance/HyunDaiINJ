using HyunDaiINJ.ViewModels.Monitoring;
using HyunDaiINJ.ViewModels.MQTT;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HyunDaiINJ.Views.Monitoring.Pages
{
    /// <summary>
    /// Monitoring.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Monitoring : Page
    {
        public Monitoring()
        {
            InitializeComponent();

            var mqttModel = new MQTTModel();

            var mqttViewModel = new MqttViewModel(mqttModel);

            DataContext = mqttViewModel;
        }
    }
}
