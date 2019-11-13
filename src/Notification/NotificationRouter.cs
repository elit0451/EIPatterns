using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Notification
{
    public static class NotificationRouter
    {
        public static void Route(string message, string correlationId)
        {
            JObject receivedObj = JsonConvert.DeserializeObject<JObject>(message);
            string command = receivedObj["command"].Value<string>();
            Console.WriteLine("Command IS: " + command);
            switch (command)
            {
                case "PackageOffers":
                    MessageGateway.SendPackageOffersNotification(message, correlationId);
                    break;
                default:
                    Console.WriteLine("No such command");
                    break;
            }
        }
    }
}