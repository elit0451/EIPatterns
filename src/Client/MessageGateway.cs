using System;
using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client
{
    public static class MessageGateway
    {
        public static readonly string notificationCorrelationId = Guid.NewGuid().ToString();
        private static BlockingCollection<string> reservationResponse = new BlockingCollection<string>();
        private static ConnectionFactory factory;
        private static IConnection requestConnection;
        private static IModel requestChannel;

        static MessageGateway(){
            factory = new ConnectionFactory() { HostName = "localhost" };
            requestConnection = factory.CreateConnection();
            requestChannel = requestConnection.CreateModel();
        }

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
            details.Add("clientID", ClientInfo.ID);

            RpcReservationRequests();

            var messageBytes = Encoding.UTF8.GetBytes(details.ToString());
            requestChannel.BasicPublish(
                exchange: "",
                routingKey: "reservations_requests",
                basicProperties: props,
                body: messageBytes);

            requestChannel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return reservationResponse.Take();
        }
        private static void RpcReservationRequests()
        {
            replyQueueName = requestChannel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(requestChannel);

            props = requestChannel.CreateBasicProperties();
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

            requestChannel.QueueDeclare(queue: "carType_request",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var clientProps = requestChannel.CreateBasicProperties();
            clientProps.CorrelationId = notificationCorrelationId;

            var body = Encoding.UTF8.GetBytes(selectedType.ToString());

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "carType_request",
                                    basicProperties: clientProps,
                                    body: body);
            
        }

        public static void ReceiveNotifications()
        {
            requestChannel.QueueDeclare(queue: "notifications.send",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var consumer = new EventingBasicConsumer(requestChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                string correlationId = ea.BasicProperties.CorrelationId;
                var message = Encoding.UTF8.GetString(body);

                if (correlationId == notificationCorrelationId)
                {
                    CommandRouter.Route(message);
                }
            };
            requestChannel.BasicConsume(queue: "notifications.send",
                                    autoAck: true,
                                    consumer: consumer);
        }

        internal static void SendPackageAccepted(Package package)
        {
            JObject clientInfo = JObject.FromObject(package);

            clientInfo.Add("clientID", ClientInfo.ID);

            requestChannel.QueueDeclare(queue: "reservations_accepted",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var clientProps = requestChannel.CreateBasicProperties();
            clientProps.CorrelationId = notificationCorrelationId;

            var body = Encoding.UTF8.GetBytes(clientInfo.ToString());

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "reservations_accepted",
                                    basicProperties: clientProps,
                                    body: body);
            
        }

        internal static void SendReservationCanceled()
        {
            JObject clientInfo = new JObject();

            clientInfo.Add("clientID", ClientInfo.ID);

            requestChannel.QueueDeclare(queue: "reservations_canceled",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

            var clientProps = requestChannel.CreateBasicProperties();
            clientProps.CorrelationId = notificationCorrelationId;

            var body = Encoding.UTF8.GetBytes(clientInfo.ToString());

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "reservations_canceled",
                                    basicProperties: clientProps,
                                    body: body);
            
        }
        
        internal static void SendCreditCardInformation(string creditCard)
        {
            requestChannel.ExchangeDeclare(exchange: "payments",
                                    type: "direct");

            var clientProps = requestChannel.CreateBasicProperties();
            clientProps.CorrelationId = notificationCorrelationId;


            JObject reservationInfo = new JObject();

            reservationInfo.Add("clientID", ClientInfo.ID);
            var reservationBody = Encoding.UTF8.GetBytes(reservationInfo.ToString());

            string payMessage = creditCard;
            var payBody = Encoding.UTF8.GetBytes(payMessage);

            requestChannel.BasicPublish(exchange: "payments",
                                 routingKey: "reservations",
                                 basicProperties: clientProps,
                                 body: reservationBody);

            requestChannel.BasicPublish(exchange: "payments",
                                 routingKey: "creditcards",
                                 basicProperties: clientProps,
                                 body: payBody);

            Console.WriteLine("Sending cc");
        }
    }
}