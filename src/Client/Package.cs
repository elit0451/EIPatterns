using System;

namespace Client
{
    public class Package
    {
        public Guid Id { get; set; }
        public double Price { get; set; }
        public string Details { get; set; }

        public Package(Guid id, double price, string details)
        {
            Id = id;
            Price = price;
            Details = details;
        }
    }
}