using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Package
{
    public static class MessageGateway
    {
        public static void ReceiveCarTypeRequests()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "carType_request",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    string correlationId = ea.BasicProperties.CorrelationId;
                    
                    JObject receivedObj = JsonConvert.DeserializeObject<JObject>(message);
                    string carType = receivedObj["carType"].Value<string>();
                    
                    SendPackageNotification(carType, correlationId);
                };
                channel.BasicConsume(queue: "carType_request",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        public static void SendPackageNotification(string carType, string correlationId)
        {
            JObject packageOffers = PackageSelector.GetAvailablePackages(carType);
            packageOffers.Add("command", "PackageOffers");

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "notifications",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var props = channel.CreateBasicProperties();
                props.CorrelationId = correlationId;

                var body = Encoding.UTF8.GetBytes(packageOffers.ToString());

                channel.BasicPublish(exchange: "",
                                     routingKey: "notifications",
                                     basicProperties: props,
                                     body: body);
            }
        }
    }
}