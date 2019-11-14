using System;
using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Reservation
{
    public static class MessageGateway
    {
        private static BlockingCollection<string> carsResponses = new BlockingCollection<string>();


        private static IConnection connection;
        private static IModel channel;
        private static string replyQueueName;
        private static EventingBasicConsumer consumer;
        private static IBasicProperties props;
        private static ConnectionFactory factory;
        private static IConnection requestConnection;
        private static IModel requestChannel;

        static MessageGateway(){
            factory = new ConnectionFactory() { HostName = "localhost" };
            requestConnection = factory.CreateConnection();
            requestChannel = requestConnection.CreateModel();
        }
        public static void ReceiveRequests()
        {
            requestChannel.QueueDeclare(queue: "reservations_requests", durable: false,
                exclusive: false, autoDelete: false, arguments: null);
            requestChannel.BasicQos(0, 1, false);
            var requestConsumer = new EventingBasicConsumer(requestChannel);
            requestChannel.BasicConsume(queue: "reservations_requests",
                autoAck: false, consumer: requestConsumer);

            requestConsumer.Received += (model, ea) =>
            {
                string response = null;

                var body = ea.Body;
                var requestProps = ea.BasicProperties;
                var replyProps = requestChannel.CreateBasicProperties();
                replyProps.CorrelationId = requestProps.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    JObject reservationData = JsonConvert.DeserializeObject<JObject>(message);
                    DateTime dateFrom = DateTime.ParseExact(reservationData["dateFrom"].Value<string>(), "dd/MM/yyyy", null);
                    DateTime dateTo = DateTime.ParseExact(reservationData["dateTo"].Value<string>(), "dd/MM/yyyy", null);
                    string locationFrom = reservationData["locFrom"].Value<string>();
                    string locationTo = reservationData["locTo"].Value<string>();
                    Guid clientId = Guid.Parse(reservationData["clientID"].Value<string>());

                    Validator.ValidateReservationData(dateFrom, dateTo);

                    Reservation r = new Reservation(clientId, Guid.Empty, false, dateFrom, dateTo, locationFrom, locationTo);
                    ReservationRepositoy.Instance.AddReservation(r);

                    FetchCars();
                    JObject carMessage = new JObject();
                    carMessage.Add("command", "fetchAllTypes");

                    var messageBytes = Encoding.UTF8.GetBytes(carMessage.ToString());
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "cars_queue",
                        basicProperties: props,
                        body: messageBytes);

                    channel.BasicConsume(
                        consumer: consumer,
                        queue: replyQueueName,
                        autoAck: true);

                    response = carsResponses.Take();

                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] " + e.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    requestChannel.BasicPublish(exchange: "", routingKey: requestProps.ReplyTo,
                        basicProperties: replyProps, body: responseBytes);
                    requestChannel.BasicAck(deliveryTag: ea.DeliveryTag,
                        multiple: false);
                }
            };
        }

        private static void FetchCars()
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
                    carsResponses.Add(response);
                }
            };
        }

        internal static void ReceiveCancelations()
        {
            requestChannel.QueueDeclare(queue: "reservations_canceled",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var consumer = new EventingBasicConsumer(requestChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                JObject clientData = JsonConvert.DeserializeObject<JObject>(message);
                Guid clientId = Guid.Parse(clientData["clientID"].Value<string>());

                Reservation r = ReservationRepositoy.Instance.CancelReservation(clientId);
                SendCancelationNotification(r, ea.BasicProperties.CorrelationId);

            };
            requestChannel.BasicConsume(queue: "reservations_canceled",
                                    autoAck: true,
                                    consumer: consumer);
        }

        internal static void ReceiveAcceptedOffer()
        {
            requestChannel.QueueDeclare(queue: "reservations_accepted",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var consumer = new EventingBasicConsumer(requestChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                JObject clientData = JsonConvert.DeserializeObject<JObject>(message);
                Guid clientId = Guid.Parse(clientData["clientID"].Value<string>());
                Guid packageId = Guid.Parse(clientData["Id"].Value<string>());
                double value = clientData["Price"].Value<double>();

                Reservation r = ReservationRepositoy.Instance.GetReservation(clientId);
                r.PackageID = packageId;
                SendPaymentRequest(value, ea.BasicProperties.CorrelationId);

            };
            requestChannel.BasicConsume(queue: "reservations_accepted",
                                    autoAck: true,
                                    consumer: consumer);
        }

        private static void SendPaymentRequest(double value, string correlationId)
        {
            string message = value.ToString();
            
            requestChannel.QueueDeclare(queue: "payment_requests",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var props = requestChannel.CreateBasicProperties();
            props.CorrelationId = correlationId;

            var body = Encoding.UTF8.GetBytes(message);

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "payment_requests",
                                    basicProperties: props,
                                    body: body);
        }

        public static void SendCancelationNotification(Reservation r, string correlationId)
        {
            string message = "Reservation ID: " + r.ReservationID + " - was canceled!";
            
            JObject clientData = new JObject();
            clientData.Add("message", message);
            clientData.Add("command", "ReservationCanceled");
            
            requestChannel.QueueDeclare(queue: "notifications",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var props = requestChannel.CreateBasicProperties();
            props.CorrelationId = correlationId;

            var body = Encoding.UTF8.GetBytes(clientData.ToString());

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "notifications",
                                    basicProperties: props,
                                    body: body);
        }

        public static void ReceivePayedReservations(){
            requestChannel.ExchangeDeclare(exchange: "payments",
                                    type: "direct");
            
            var queueName = requestChannel.QueueDeclare().QueueName;

            requestChannel.QueueBind(queue: queueName,
                            exchange: "payments",
                            routingKey: "reservations");

            var consumer = new EventingBasicConsumer(requestChannel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                JObject clientData = JsonConvert.DeserializeObject<JObject>(message);
                Guid clientId = Guid.Parse(clientData["clientID"].Value<string>());

                ReservationRepositoy.Instance.SetPaid(clientId);
                Reservation r = ReservationRepositoy.Instance.GetReservation(clientId);

                SendSuccessfulBookingNotification(r, ea.BasicProperties.CorrelationId);
            };

            requestChannel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        private static void SendSuccessfulBookingNotification(Reservation r, string correlationId)
        {
            string message = "Reservation ID: " + r.ReservationID + " - was sucessfuly booked!";
            
            JObject clientData = new JObject();
            clientData.Add("message", message);
            clientData.Add("command", "SuccessfulBooking");
            
            requestChannel.QueueDeclare(queue: "notifications",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var props = requestChannel.CreateBasicProperties();
            props.CorrelationId = correlationId;

            var body = Encoding.UTF8.GetBytes(clientData.ToString());

            requestChannel.BasicPublish(exchange: "",
                                    routingKey: "notifications",
                                    basicProperties: props,
                                    body: body);
        }
    }
}