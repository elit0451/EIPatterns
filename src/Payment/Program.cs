using System;

namespace Payment
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            MessageGateway.ReceivePaymentRequests(); 
            MessageGateway.ReceiveCreditCardInfo(); 

            Console.WriteLine("Running Payment service...");
            Console.ReadLine();
        }
    }
}
