using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Car
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

        public static void ReceiveRequests()
        {
            
            requestChannel.QueueDeclare(queue: "cars_queue", durable: false,
                exclusive: false, autoDelete: false, arguments: null);
            requestChannel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(requestChannel);
            requestChannel.BasicConsume(queue: "cars_queue",
                autoAck: false, consumer: consumer);
            Console.WriteLine(" [x] Awaiting for car requests");

            consumer.Received += (model, ea) =>
            {
                string response = null;

                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = requestChannel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    JObject requestObj = JsonConvert.DeserializeObject<JObject>(message);
                    string requestCommand = requestObj["command"].Value<string>();

                    response = CommandController.ExecuteCommand(requestCommand);
                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] " + e.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    requestChannel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                        basicProperties: replyProps, body: responseBytes);
                    requestChannel.BasicAck(deliveryTag: ea.DeliveryTag,
                        multiple: false);
                }
            };  
        }
    }
}