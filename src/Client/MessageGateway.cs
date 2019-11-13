using System;
using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client
{
    public static class MessageGateway
    {
        public static readonly string notificationCorrelationId = Guid.NewGuid().ToString();
        private static BlockingCollection<string> reservationResponse = new BlockingCollection<string>();
        private static IConnection connection;
        private static IModel channel;
        private static string replyQueueName;
        private static EventingBasicConsumer consumer;
        private static IBasicProperties props;

        public static string SendReservationDetails(string locationFrom, string locationTo, DateTime dateFrom, DateTime dateTo)
        {
            JObject details = new JObject();

            details.Add("locFrom", locationFrom);
            details.Add("locTo", locationTo);
            details.Add("dateFrom", dateFrom.ToString("dd/MM/yyyy"));
            details.Add("dateTo", dateTo.ToString("dd/MM/yyyy"));

            RpcReservationRequests();

            var messageBytes = Encoding.UTF8.GetBytes(details.ToString());
            channel.BasicPublish(
                exchange: "",
                routingKey: "reservations_requests",
                basicProperties: props,
                body: messageBytes);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return reservationResponse.Take();
        }
        private static void RpcReservationRequests()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    reservationResponse.Add(response);
                }
            };
        }
        internal static void SendCarType(string carType)
        {
            JObject selectedType = new JObject();

            selectedType.Add("carType", carType);

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "carType_request",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var clientProps = channel.CreateBasicProperties();
                clientProps.CorrelationId = notificationCorrelationId;

                var body = Encoding.UTF8.GetBytes(selectedType.ToString());

                channel.BasicPublish(exchange: "",
                                     routingKey: "carType_request",
                                     basicProperties: clientProps,
                                     body: body);
            }
        }

        public static void ReceiveNotifications()
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

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    string correlationId = ea.BasicProperties.CorrelationId;
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine(notificationCorrelationId);
                    Console.WriteLine(ea.BasicProperties.CorrelationId);

                    if (correlationId == notificationCorrelationId)
                        Console.WriteLine(message);
                };
                channel.BasicConsume(queue: "notifications.send",
                                     autoAck: true,
                                     consumer: consumer);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}