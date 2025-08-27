using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.ShowtimeAggregate.Enum
{
    public enum ShowtimeStatus
    {
        Scheduled = 1, // Showtime is scheduled but not yet confirmed
        Confirmed = 2, // Showtime is confirmed and ready for bookings
        Cancelled = 3, // Showtime has been cancelled
        Completed = 4, // Showtime has been completed
    }
}
