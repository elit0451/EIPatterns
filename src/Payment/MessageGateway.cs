using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Payment
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
        
        internal static void ReceivePaymentRequests()
        {
            requestChannel.QueueDeclare(queue: "payment_requests",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            var consumer = new EventingBasicConsumer(requestChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                double price = double.Parse(message);
                SendCreditCardNotification(price, ea.BasicProperties.CorrelationId);

            };
            requestChannel.BasicConsume(queue: "payment_requests",
                                    autoAck: true,
                                    consumer: consumer);
        }

        public static void SendCreditCardNotification(double price, string correlationId)
        {
            string message = "Pay " + price + "$ - Insert credit card number:";
            
            JObject clientData = new JObject();
            clientData.Add("message", message);
            clientData.Add("command", "CreditCardRequest");
            
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

        public static void ReceiveCreditCardInfo(){
            requestChannel.ExchangeDeclare(exchange: "payments",
                                    type: "direct");
            
            var queueName = requestChannel.QueueDeclare().QueueName;

            requestChannel.QueueBind(queue: queueName,
                            exchange: "payments",
                            routingKey: "creditcards");

            var consumer = new EventingBasicConsumer(requestChannel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                bool successPayment = new Random().Next(100) <= 50 ? true : false;

                SendPaymentStatusNotification(successPayment, ea.BasicProperties.CorrelationId);
            };

            requestChannel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        private static void SendPaymentStatusNotification(bool successPayment, string correlationId)
        {
            string message = successPayment.ToString();
            
            JObject clientData = new JObject();
            clientData.Add("message", message);
            clientData.Add("command", "PaymentStatus");
            
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