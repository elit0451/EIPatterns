using System;

namespace Package
{
    class Program
    {
        public static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            MessageGateway.ReceiveCarTypeRequests();
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
