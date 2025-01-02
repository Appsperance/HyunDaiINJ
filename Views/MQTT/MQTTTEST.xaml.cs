using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HyunDaiINJ.ViewModels.MQTT;

namespace HyunDaiINJ.Views.MQTT
{
    public partial class MQTTTEST : Page
    {
        private readonly MqttViewModel _viewModel;

        public MQTTTEST()
        {
            InitializeComponent();
            _viewModel = new MqttViewModel();
            DataContext = _viewModel;
        }

        private async void ConnectAndSubscribeButton_Click(object sender, RoutedEventArgs e)
        {
            string brokerAddress = "43.203.159.137";
            string username = "admin";
            string password = "vapor";

            try
            {
                await _viewModel.ConnectAndSubscribeAsync(brokerAddress, username, password);
                if (_viewModel.IsConnected)
                {
                    MessageBox.Show("MQTT 연결 성공!", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("MQTT 연결 실패!", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"연결 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
