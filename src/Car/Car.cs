namespace Car
{
    public class Car
    {
        public string Brand { get; set; }
        public int SeatsNr { get; set; }
        public string Model { get; set; }

        public Car(string brand, int seatNr, string model)
        {
            Brand = brand;
            SeatsNr = seatNr;
            Model = model;
        }
    }
}