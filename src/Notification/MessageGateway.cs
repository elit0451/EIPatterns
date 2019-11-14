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
        private static ConnectionFactory factory;
        private static IConnection requestConnection;
        private static IModel requestChannel;

        static MessageGateway(){
            factory = new ConnectionFactory() { HostName = "localhost" };
            requestConnection = factory.CreateConnection();
            requestChannel = requestConnection.CreateModel();
        }

        internal static void ReceiveNotifications()
        {
            
            requestChannel.QueueDeclare(queue: "notifications",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var consumer = new EventingBasicConsumer(requestChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(message);
                NotificationRouter.Route(message, ea.BasicProperties.CorrelationId);

            };
            requestChannel.BasicConsume(queue: "notifications",
                                    autoAck: true,
                                    consumer: consumer);
        }


        public static void SendPackageOffersNotification(string message, string correlationId)
        {
            requestChannel.QueueDeclare(queue: "notifications.send",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            var props = requestChannel.CreateBasicProperties();
            props.CorrelationId = correlationId;

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "notifications.send",
                                    basicProperties: props,
                                    body: body);
        }
        internal static void SendReservationCanceledNotification(string message, string correlationId)
        {
            requestChannel.QueueDeclare(queue: "notifications.send",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            var props = requestChannel.CreateBasicProperties();
            props.CorrelationId = correlationId;

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "notifications.send",
                                    basicProperties: props,
                                    body: body);
        }

        internal static void SendCreditCardRequestNotification(string message, string correlationId)
        {
            requestChannel.QueueDeclare(queue: "notifications.send",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            var props = requestChannel.CreateBasicProperties();
            props.CorrelationId = correlationId;

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "notifications.send",
                                    basicProperties: props,
                                    body: body);
        }
        
        internal static void SendPaymentStatusNotification(string message, string correlationId)
        {
            requestChannel.QueueDeclare(queue: "notifications.send",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            var props = requestChannel.CreateBasicProperties();
            props.CorrelationId = correlationId;

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "notifications.send",
                                    basicProperties: props,
                                    body: body);
        }
        
        internal static void SendSuccessfulBookingNotification(string message, string correlationId)
        {
            requestChannel.QueueDeclare(queue: "notifications.send",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            var props = requestChannel.CreateBasicProperties();
            props.CorrelationId = correlationId;

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "notifications.send",
                                    basicProperties: props,
                                    body: body);
        }
    }
}