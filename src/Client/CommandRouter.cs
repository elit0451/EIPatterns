using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client
{
    public static class CommandRouter
    {
        public static void Route(string message)
        {
            JObject receivedObj = JsonConvert.DeserializeObject<JObject>(message);
            string command = receivedObj["command"].Value<string>();
            string msg = "";
            switch (command)
            {
                case "PackageOffers":
                    List<Package> packages = JsonConvert.DeserializeObject<List<Package>>(receivedObj["packages"].ToString());
                    UIHelper.PrintOfferPackages(packages);
                    break;
                case "ReservationCanceled":
                    msg = receivedObj["message"].ToString();
                    UIHelper.PrintReservationCanceled(msg);
                    break;
                case "CreditCardRequest":
                    msg = receivedObj["message"].ToString();
                    UIHelper.RequestCreditCard(msg);
                    break;
                case "PaymentStatus":
                    bool successPayment = bool.Parse(receivedObj["message"].ToString());
                    UIHelper.PaymentStatus(successPayment);
                    break;
                case "SuccessfulBooking":
                    msg = receivedObj["message"].ToString();
                    UIHelper.SuccessfulBooking(msg);
                    break;
                default:
                    Console.WriteLine("No such command");
                    break;
            }   
        }
    }
}