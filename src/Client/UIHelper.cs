using System;
using System.Collections.Generic;

namespace Client
{
    public static class UIHelper
    {
        public static void PrintOfferPackages(List<Package> packages)
        {
            foreach(Package p in packages){
                Console.WriteLine(p.Price + "$ - " + p.Details);
            }
            Console.WriteLine("Choose a package: ");
            int selection = int.Parse(Console.ReadLine());

            try
            {
                MessageGateway.SendPackageAccepted(packages.ToArray()[selection-1]);
            }
            catch(Exception e)
            {
                MessageGateway.SendReservationCanceled();
            }
        }

        internal static void PrintReservationCanceled(string msg)
        {
            Console.WriteLine(msg);
        }

        internal static void RequestCreditCard(string msg)
        {
            Console.WriteLine(msg);
            string creditCard = Console.ReadLine();
            MessageGateway.SendCreditCardInformation(creditCard);
        }

        internal static void PaymentStatus(bool successPayment)
        {

            if(successPayment)
                Console.WriteLine("Payment sucessful!");
            else
            {
                Console.WriteLine("Payment not sucessful!");
                MessageGateway.SendReservationCanceled();
            }
        }

        internal static void SuccessfulBooking(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}