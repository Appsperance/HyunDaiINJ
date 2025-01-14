using MQTTnet.Client;
using MQTTnet;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MQTTnet.Server;
using System.Diagnostics;
using System.Linq;
using HyunDaiINJ.DATA.DTO;

public class MQTTModel
{
    private IMqttClient? mqttClient;
    public event Action<string, MqttVisionDTO>? VisionMessageReceived;
    public event Action<string, MqttProcessDTO>? ProcessMessageReceived;



    public bool mqttConnected => mqttClient?.IsConnected ?? false;
    public async Task MqttConnect()
    {
        var factory = new MqttFactory();
        mqttClient = factory.CreateMqttClient();

        // 메시지 수신 이벤트 핸들러 등록
        mqttClient.ApplicationMessageReceivedAsync += MqttRecivedMessage;

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("43.203.159.137", 1883)
            .WithCredentials("admin", "vapor")
            .WithCleanSession(true)
            .Build();

        try
        {
            await mqttClient.ConnectAsync(options);

            // 연결 상태 확인
            if (mqttClient.IsConnected)
            {
                // 연결 성공 상태만 반환
                Console.WriteLine("[MqttService] MQTT 연결이 활성화되었습니다.");
            }
            else
            {
                Console.WriteLine("[MqttService] MQTT 연결이 활성화되지 않았습니다.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MQTT 연결 실패: {ex.Message}");
        }
    }

    public async Task SubscribeMQTT(string topic)
    {
        if (mqttClient?.IsConnected == true)
        {
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build());
            Console.WriteLine($"[MqttService] 토픽 구독 성공: {topic}");
        }
        else
        {
            Console.WriteLine("[MqttService] MQTT 연결이 활성화되지 않았습니다.");
        }
    }

    public async Task MqttRecivedMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        await Task.Run(() =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());

            try
            {
                if (topic.StartsWith("Vision/ng"))
                {
                    var visionNg = JsonConvert.DeserializeObject<MqttVisionDTO>(payload);
                    if (visionNg != null)
                    {
                        VisionMessageReceived?.Invoke(topic, visionNg);
                        Console.WriteLine($"{visionNg}");
                    }
                }

                if (topic.StartsWith("Process/PLC"))
                {
                    var processPlc = JsonConvert.DeserializeObject<MqttProcessDTO>(payload);
                    if (processPlc != null)
                    {
                        ProcessMessageReceived?.Invoke(topic, processPlc);
                        Console.WriteLine($"{processPlc}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MqttService] 역직렬화 실패: {ex.Message}");
            }
        });
    }
}
