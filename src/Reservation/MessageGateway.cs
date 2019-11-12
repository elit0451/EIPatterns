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

        public static void ReceiveRequests()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var requestConnection = factory.CreateConnection())
            using (var requestChannel = requestConnection.CreateModel())
            {
                requestChannel.QueueDeclare(queue: "reservations_requests", durable: false,
                  exclusive: false, autoDelete: false, arguments: null);
                requestChannel.BasicQos(0, 1, false);
                var requestConsumer = new EventingBasicConsumer(requestChannel);
                requestChannel.BasicConsume(queue: "reservations_requests",
                  autoAck: false, consumer: requestConsumer);
                Console.WriteLine(" [x] Awaiting for requests");

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

                        Validator.ValidateReservationData(dateFrom, dateTo);

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

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
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

    }
}