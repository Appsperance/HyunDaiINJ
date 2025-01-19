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
using HyunDaiINJ.ViewModels.Monitoring.vision;
using HyunDaiINJ.Views.Monitoring.Controls.Vision;

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

            var mqttViewModel = new MqttVisionViewModel(mqttModel);

            this.DataContext = mqttViewModel;

        }

        // Border 클릭 시 발생
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border
                && border.Child is Grid grid
                && grid.Children.Count > 0
                && grid.Children[0] is Image image
                && image.Source is BitmapImage bitmapImage)
            {
                // (1) NgCard 창 만들기
                var ngCardWindow = new NgCard(bitmapImage);

                // (2) 창 보여주기
                ngCardWindow.Show();
            }
        }
    }
}
