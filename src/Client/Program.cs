using System;
using System.Xml.Linq;
using System.Linq;

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
            string carTypes = MessageGateway.SendReservationDetails("Sofia", "Porto", new DateTime(2019, 11, 17), new DateTime(2020, 06, 21));
            var doc = XDocument.Parse(carTypes);

            foreach (XElement type in doc.Root.Elements("Type"))
                Console.WriteLine(type.Value);

            Console.WriteLine("Select the type of car you need:");
            int selection = int.Parse(Console.ReadLine());

            MessageGateway.SendCarType(doc.Root.Elements("Type").ElementAt(selection - 1).Value);


            MessageGateway.ReceiveNotifications();
            while(true){
            }

        }


    }
}
