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
        private static ConnectionFactory factory;
        private static IConnection requestConnection;
        private static IModel requestChannel;

        static MessageGateway(){
            factory = new ConnectionFactory() { HostName = "localhost" };
            requestConnection = factory.CreateConnection();
            requestChannel = requestConnection.CreateModel();
        }
        
        public static void ReceiveCarTypeRequests()
        {
            requestChannel.QueueDeclare(queue: "carType_request",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var consumer = new EventingBasicConsumer(requestChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                string correlationId = ea.BasicProperties.CorrelationId;
                
                JObject receivedObj = JsonConvert.DeserializeObject<JObject>(message);
                string carType = receivedObj["carType"].Value<string>();
                
                SendPackageNotification(carType, correlationId);
            };
            requestChannel.BasicConsume(queue: "carType_request",
                                    autoAck: true,
                                    consumer: consumer);
        }

        public static void SendPackageNotification(string carType, string correlationId)
        {
            JObject packageOffers = PackageSelector.GetAvailablePackages(carType);
            packageOffers.Add("command", "PackageOffers");
            requestChannel.QueueDeclare(queue: "notifications",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var props = requestChannel.CreateBasicProperties();
            props.CorrelationId = correlationId;

            var body = Encoding.UTF8.GetBytes(packageOffers.ToString());

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "notifications",
                                    basicProperties: props,
                                    body: body);
        }
    }
}