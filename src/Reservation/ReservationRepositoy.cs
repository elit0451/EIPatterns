using System;
using System.Collections.Generic;
using System.Linq;

namespace Reservation
{
    public class ReservationRepositoy
    {
        private List<Reservation> packageRepo;
        private static ReservationRepositoy _instance;
        public static ReservationRepositoy Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new ReservationRepositoy();
                return _instance;
            }
        }

        private ReservationRepositoy()
        {
            packageRepo = new List<Reservation>();
        }

        public void AddReservation(Reservation r)
        {
            packageRepo.Add(r);
        }

        public Reservation GetReservation(Guid clientID){
            return packageRepo.Where(x => x.ClientID == clientID).First();
        }

        public void SetPaid(Guid clientID)
        {
            packageRepo.Where(x => x.ClientID == clientID).First().Paid = true;
        }

        public Reservation CancelReservation(Guid clientID){
            Reservation r = packageRepo.Where(x => x.ClientID == clientID).First();
            packageRepo.Remove(r);
            return r;
        }

    }
}