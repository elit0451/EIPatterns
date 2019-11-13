using System;

namespace Reservation
{
    public static class Validator
    {
        public static void ValidateReservationData(DateTime dateFrom, DateTime dateTo)
        {
            if (!(dateFrom >= DateTime.Now && dateFrom < dateTo))
                throw new ArgumentException("Incorrect dates.");
        }
    }
}