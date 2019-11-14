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
            switch (command)
            {
                case "PackageOffers":
                    MessageGateway.SendPackageOffersNotification(message, correlationId);
                    break;
                case "ReservationCanceled":
                    MessageGateway.SendReservationCanceledNotification(message, correlationId);
                    break;
                case "CreditCardRequest":
                    MessageGateway.SendCreditCardRequestNotification(message, correlationId);
                    break;
                case "PaymentStatus":
                    MessageGateway.SendPaymentStatusNotification(message, correlationId);
                    break;
                case "SuccessfulBooking":
                    MessageGateway.SendSuccessfulBookingNotification(message, correlationId);
                    break;
                default:
                    Console.WriteLine("No such command");
                    break;
            }
        }
    }
}