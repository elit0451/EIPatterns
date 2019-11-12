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
    }
}