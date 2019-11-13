using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification
{
    internal static class MessageGateway
    {
        internal static void ReceiveNotifications()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "notifications",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);

                    NotificationRouter.Route(message, ea.BasicProperties.CorrelationId);

                };
                channel.BasicConsume(queue: "notifications",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        public static void SendPackageOffersNotification(string message, string correlationId)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "notifications.send",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);
                var props = channel.CreateBasicProperties();
                props.CorrelationId = correlationId;

                Console.WriteLine("CorrelationID: " + correlationId);

                channel.BasicPublish(exchange: "",
                                     routingKey: "notifications.send",
                                     basicProperties: props,
                                     body: body);
            }
        }
    }
}