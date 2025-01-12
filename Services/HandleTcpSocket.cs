using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HyunDaiINJ.DATA.DTO;

namespace HyunDaiINJ.Services
{
    public class HandleTcpSocket
    {
        private const string ServerIp = "127.0.0.1";
        private const int ServerPort = 51900;

        // 기존 (단건 DTO -> JSON) 전송
        public async Task SendDataAsync(InjectionPlanDTO data)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ServerIp, ServerPort);

                using var networkStream = client.GetStream();

                string jsonData = JsonSerializer.Serialize(data);
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);

                await networkStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                Console.WriteLine($"데이터(단건) 전송 성공: {jsonData}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP 소켓 오류(단건): {ex.Message}");
            }
        }

        // 기존 (복수 DTO -> JSON) 전송
        public async Task SendMultipleAsync(List<InjectionPlanDTO> dataList)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ServerIp, ServerPort);

                using var networkStream = client.GetStream();

                string jsonData = JsonSerializer.Serialize(dataList);
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);

                await networkStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                Console.WriteLine($"데이터(복수) 전송 성공: {jsonData}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP 소켓 오류(복수): {ex.Message}");
            }
        }

        // [새 오버로드] 바이너리 패킷 전송 (Header+Payload)
        public async Task SendMultipleAsync(byte[] rawPacket)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ServerIp, ServerPort);

                using var stream = client.GetStream();

                // rawPacket 그대로 전송
                await stream.WriteAsync(rawPacket, 0, rawPacket.Length);

                Console.WriteLine($"[SendMultipleAsync(byte[])] 바이너리 패킷 전송: 길이={rawPacket.Length}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP 소켓 오류(바이너리): {ex.Message}");
            }
        }
    }
}
