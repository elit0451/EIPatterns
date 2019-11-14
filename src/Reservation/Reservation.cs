using System;

namespace Reservation
{
    public class Reservation
    {
        public Guid ReservationID { get; set; }
        public Guid ClientID { get; set; }
        public Guid PackageID { get; set; }
        public bool Paid { get; set; } = false;
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public Reservation(Guid clientID, Guid packageID, bool paid, DateTime dateFrom, DateTime dateTo, string from, string to)
        {
            ReservationID = Guid.NewGuid();
            ClientID = clientID;
            PackageID = packageID;
            Paid = paid;
            DateFrom = dateFrom;
            DateTo = dateTo;
            From = from;
            To = to;
        }
    }
}