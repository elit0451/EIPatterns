using System;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            Console.WriteLine(MessageGateway.SendReservationDetails("Sofia", "Porto", new DateTime(2019, 11, 13), new DateTime(2020, 06, 21)));
        }
    }
}
