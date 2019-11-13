using System;

namespace Package
{
    public class Package
    {
        public Guid Id { get; set; }
        public double Price { get; set; }
        public string Details { get; set; }

        public Package(double price, string details)
        {
            Id = Guid.NewGuid();
            Price = price;
            Details = details;
        }
    }
}