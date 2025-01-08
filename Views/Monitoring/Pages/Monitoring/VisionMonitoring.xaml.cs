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
using HyunDaiINJ.ViewModels.MQTT;
using HyunDaiINJ.Views.Monitoring.Controls.ThirdSection.Vision;

namespace HyunDaiINJ.Views.Monitoring.Pages.Monitoring
{
    /// <summary>
    /// VisionMonitoring.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VisionMonitoring : Page
    {
        public VisionMonitoring()
        {
            InitializeComponent();

            var mqttModel = new MQTTModel();

            var mqttViewModel = new MqttViewModel(mqttModel);

            DataContext = mqttViewModel;

            NgCard.DataContext = mqttViewModel;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Child is Grid grid && grid.Children[0] is Image image && image.Source is BitmapImage bitmapImage)
            {
                // NgCard의 SelectedImage를 설정
                if (NgCard.FindName("SelectedImage") is Image selectedImage)
                {
                    selectedImage.Source = bitmapImage;
                }
            }
        }
    }
}
