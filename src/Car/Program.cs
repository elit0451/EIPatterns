using System;

namespace Car
{
    class Program
    {
        public static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
             MessageGateway.ReceiveRequests();
            Console.WriteLine("Running Car service...");
            Console.ReadLine();
        }
    }
}
