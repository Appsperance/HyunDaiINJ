using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HyunDaiINJ.ViewModels.Monitoring.vision;
using HyunDaiINJ.Views.Monitoring.Controls.Vision;
// 만약 NgCard(Window) 클래스가 있다면 using HyunDaiINJ.Views.Monitoring.Controls.Vision;

namespace HyunDaiINJ.Views.Monitoring.Pages.Monitoring
{
    public partial class VisionMonitoring : Page
    {
        // 5개 ViewModel 프로퍼티
        public MqttVisionViewModel VisionVM1 { get; }
        public MqttVisionViewModel VisionVM2 { get; }
        public MqttVisionViewModel VisionVM3 { get; }
        public MqttVisionViewModel VisionVM4 { get; }
        public MqttVisionViewModel VisionVM5 { get; }

        public VisionMonitoring()
        {
            InitializeComponent();

            // 1) 비전1~5 각각 전용 MQTTModel & ViewModel
            var mqtt1 = new MQTTModel();
            VisionVM1 = new MqttVisionViewModel(mqtt1, "Vision/ng");

            var mqtt2 = new MQTTModel();
            VisionVM2 = new MqttVisionViewModel(mqtt2, "Vision/ng/2");

            var mqtt3 = new MQTTModel();
            VisionVM3 = new MqttVisionViewModel(mqtt3, "Vision/ng/3");

            var mqtt4 = new MQTTModel();
            VisionVM4 = new MqttVisionViewModel(mqtt4, "Vision/ng/4");

            var mqtt5 = new MQTTModel();
            VisionVM5 = new MqttVisionViewModel(mqtt5, "Vision/ng/5");

            // 2) Page 자체를 DataContext로
            this.DataContext = this;
        }

        // 품질 이미지(Border) 클릭 시 이벤트
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 클릭한 Border 안의 Image Source가 BitmapImage면 새창(ngCard) 열기
            if (sender is Border border
                && border.Child is Grid grid
                && grid.Children.Count > 0
                && grid.Children[0] is Image image
                && image.Source is BitmapImage bitmapImage)
            {
                // 만약 NgCard 라는 Window로 크게 보는 기능이 있다면:
                var ngCardWindow = new NgCard(bitmapImage);
                ngCardWindow.Show();
            }
        }
    }
}
