//using MQTTnet;
//using MQTTnet.Client;
//using System;
//using System.Text;
//using System.Threading.Tasks;

//public class MqttService
//{
//    private IMqttClient _mqttClient;

//    // 수신된 메시지를 외부로 전달하는 이벤트
//    public event Action<string, string> OnMessageReceived;

//    // RabbitMQ MQTT 브로커에 연결
//    public async Task ConnectAsync(string brokerAddress, string username, string password)
//    {
//        var factory = new MqttFactory();
//        _mqttClient = factory.CreateMqttClient();

//        var options = new MqttClientOptionsBuilder()
//            .WithTcpServer(brokerAddress, 1883) // MQTT 브로커 주소와 포트
//            .WithCredentials(username, password) // 사용자 이름과 비밀번호
//            .WithCleanSession(false) // 세션 유지
//            .Build();

//        // 메시지 수신 핸들러
//        _mqttClient.ApplicationMessageReceivedAsync += e =>
//        {
//            var topic = e.ApplicationMessage.Topic;
//            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

//            // 수신된 메시지를 로그에 출력
//            Console.WriteLine($"수신된 메시지 - 토픽: {topic}, 내용: {payload}");

//            // 이벤트로 메시지 전달
//            OnMessageReceived?.Invoke(topic, payload);
//            return Task.CompletedTask;
//        };

//        // 연결 성공/실패 핸들러
//        _mqttClient.ConnectedAsync += e =>
//        {
//            Console.WriteLine("MQTT 연결 성공");
//            return Task.CompletedTask;
//        };

//        _mqttClient.DisconnectedAsync += e =>
//        {
//            Console.WriteLine("MQTT 연결 해제");
//            return Task.CompletedTask;
//        };

//        try
//        {
//            await _mqttClient.ConnectAsync(options);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"MQTT 연결 실패: {ex.Message}");
//        }
//    }

//    // 특정 토픽 구독
//    public async Task SubscribeAsync(string topic)
//    {
//        if (_mqttClient.IsConnected)
//        {
//            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
//                .WithTopic(topic)
//                .Build());
//            Console.WriteLine($"토픽 구독 성공: {topic}");
//        }
//        else
//        {
//            Console.WriteLine("MQTT 연결이 활성화되지 않았습니다.");
//        }
//    }
//}
