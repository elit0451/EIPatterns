using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Car
{
    public class CarRepository : IDBFacade
    {
        private Dictionary<string, List<Car>> carRepo;
        private static CarRepository _instance;
        public static CarRepository Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new CarRepository();
                return _instance;
            }
        }
        private CarRepository()
        {
            carRepo = new Dictionary<string, List<Car>>();
            carRepo.Add("CityCar", new List<Car>());
            carRepo.Add("FamillyCar", new List<Car>());
            carRepo.Add("Van", new List<Car>());

            carRepo["CityCar"].Add(new Car("Mini", 4, "cooper"));
            carRepo["CityCar"].Add(new Car("Mazda", 2, "mx5"));
            carRepo["FamillyCar"].Add(new Car("Volvo", 5, "s60"));
            carRepo["FamillyCar"].Add(new Car("Audi", 5, "s6"));
            carRepo["Van"].Add(new Car("Renault", 2, "kangaroo"));
            carRepo["Van"].Add(new Car("Citroën", 2, "jumper"));
        }
        public string GetAllCars()
        {
            List<Car> carList = new List<Car>();
            carList.AddRange(carRepo["CityCar"]);
            carList.AddRange(carRepo["FamillyCar"]);
            carList.AddRange(carRepo["Van"]);

            XElement rootElm = new XElement("Cars", carList.Select((c, index) =>
                new XElement("Car",
                    new XElement("Brand", c.Brand),
                    new XElement("Model", c.Model),
                    new XElement("SeatsNr", c.SeatsNr)
                )));

            return rootElm.ToString();
        }

        public string GetAllTypes()
        {
            List<string> carList = carRepo.Keys.ToList();

            XElement rootElm = new XElement("Types", carList.Select((t, index) =>
                new XElement("Type", t)));

            return rootElm.ToString();
        }
    }
}