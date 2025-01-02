//using System;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using System.Threading.Tasks;

//namespace HyunDaiINJ.ViewModels.MQTT
//{
//    public class MqttViewModel : INotifyPropertyChanged
//    {
//        private readonly MqttService _mqttService;
//        private string _message;
//        private bool _isConnected;

//        public string Message
//        {
//            get => _message;
//            set
//            {
//                _message = value;
//                OnPropertyChanged();
//            }
//        }

//        public bool IsConnected
//        {
//            get => _isConnected;
//            set
//            {
//                _isConnected = value;
//                OnPropertyChanged();
//            }
//        }

//        public MqttViewModel()
//        {
//            _mqttService = new MqttService();
//            _mqttService.OnMessageReceived += HandleMessageReceived;
//        }

//        public async Task ConnectAndSubscribe(string brokerAddress, string username, string password, string topic)
//        {
//            await _mqttService.ConnectAsync(brokerAddress, username, password);
//            IsConnected = true;
//            await _mqttService.SubscribeAsync(topic);
//        }

//        public async Task SubscribeToAdditionalTopic(string topic)
//        {
//            if (IsConnected)
//            {
//                await _mqttService.SubscribeAsync(topic);
//            }
//            else
//            {
//                throw new InvalidOperationException("MQTT 연결이 활성화되지 않았습니다.");
//            }
//        }

//        private void HandleMessageReceived(string topic, string payload)
//        {
//            Message = $"Topic: {topic}, Message: {payload}";
//        }

//        public event PropertyChangedEventHandler PropertyChanged;

//        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//    }
//}

