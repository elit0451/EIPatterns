using System;

namespace Reservation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            MessageGateway.ReceiveRequests();
            MessageGateway.ReceiveCancelations();   
            MessageGateway.ReceiveAcceptedOffer();   
            MessageGateway.ReceivePayedReservations();   

            Console.WriteLine("Running Reservation service...");
            Console.ReadLine();
        }
    }
}
