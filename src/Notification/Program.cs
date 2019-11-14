using System;

namespace Notification
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            MessageGateway.ReceiveNotifications();
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
