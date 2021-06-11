using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MQTTClient1
{
    static class Program
    {
        private static IMqttClient mqttClient = null;
        static async Task Main()
        {
            MqttApplicationMessage receivedMessage = null;
            //創建 mqtt client
            mqttClient = new MqttFactory().CreateMqttClient() as MqttClient;

            //連線方式、client 資訊(驗證用)
            var options = new MqttClientOptionsBuilder()
            .WithTcpServer("127.0.0.1")
            .WithClientId("c001")
            .WithCredentials("u001", "p001")
            .WithCleanSession(true);

            await MqttClientExtensions.ConnectAsync(mqttClient, options.Build());
            //取得訊號動作
            mqttClient.UseApplicationMessageReceivedHandler(context =>
            {
                receivedMessage = context.ApplicationMessage;
                Console.WriteLine($"{Encoding.UTF8.GetString(context.ApplicationMessage.Payload)}");
            });
            //連線動作
            mqttClient.UseConnectedHandler(context =>
            {
                Console.WriteLine($"connected {context.AuthenticateResult}");
            });
            //斷線動作
            mqttClient.UseDisconnectedHandler(context =>
            {
                Console.WriteLine($"disconnected {context.AuthenticateResult}");
            });
            //訂閱接收 topic = "topic1"
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("topic1").WithExactlyOnceQoS().Build());
            await Task.Delay(500);
            //被發布的訊息
            var message = new MqttApplicationMessageBuilder().WithTopic("topic1").WithPayload("Hello World1").WithExactlyOnceQoS().WithRetainFlag().Build();
            //發布訊息
            await mqttClient.PublishAsync(message);
            await Task.Delay(500);
            while (true)
            {
                string inputString = Console.ReadLine()?.ToLower().Trim();
                if (inputString == "exist")
                {
                    Console.WriteLine("Server Down!");
                    await mqttClient.DisconnectAsync();
                }
                else if (inputString == "send")
                {
                    await mqttClient.PublishAsync(message);
                }
                else
                {
                    Console.WriteLine("cmd not exist!");
                }
            }
        }

    }
}
