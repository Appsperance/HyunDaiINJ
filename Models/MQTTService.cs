using MQTTnet.Client;
using MQTTnet;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using HyunDaiINJ.Dto;
using System.IO;

public class MqttService
{
    private IMqttClient? _mqttClient;

    public event Action<string, RabbitMQQualityOneDTO>? OnQualityOneMessageReceived;
    public event Action<string, RabbitMQQualityZeroDTO>? OnQualityZeroMessageReceived;

    public async Task ConnectAsync(string brokerAddress, string username, string password)
    {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerAddress, 1883)
            .WithCredentials(username, password)
            .WithCleanSession(true)
            .Build();

        Console.WriteLine($"[MqttService] MQTT 브로커 연결 시도: {brokerAddress}, 사용자: {username}");

        _mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());

            Console.WriteLine($"[MqttService] 수신된 메시지 - 토픽: {topic}, 내용: {payload}");

            // 수신된 메시지 저장 (디버깅)
            File.WriteAllText($"C:\\temp\\mqtt_message_{DateTime.Now.Ticks}.json", payload);
            Console.WriteLine($"MQTT 메시지 저장 완료: C:\\temp\\mqtt_message_{DateTime.Now.Ticks}.json");

            try
            {
                if (topic.StartsWith("Quality/one"))
                {
                    var qualityOneDto = JsonConvert.DeserializeObject<RabbitMQQualityOneDTO>(payload);
                    OnQualityOneMessageReceived?.Invoke(topic, qualityOneDto);
                }
                else if (topic.StartsWith("Quality/zero"))
                {
                    var qualityZeroDto = JsonConvert.DeserializeObject<RabbitMQQualityZeroDTO>(payload);
                    OnQualityZeroMessageReceived?.Invoke(topic, qualityZeroDto);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MqttService] 역직렬화 실패: {ex.Message}");
            }

            return Task.CompletedTask;
        };


        try
        {
            await _mqttClient.ConnectAsync(options);
            Console.WriteLine("[MqttService] MQTT 연결 성공");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MqttService] MQTT 연결 실패: {ex.Message}");
        }
    }

    public async Task SubscribeAsync(string topic)
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build());
            Console.WriteLine($"[MqttService] 토픽 구독 성공: {topic}");
        }
        else
        {
            Console.WriteLine("[MqttService] MQTT 연결이 활성화되지 않았습니다.");
        }
    }
}
