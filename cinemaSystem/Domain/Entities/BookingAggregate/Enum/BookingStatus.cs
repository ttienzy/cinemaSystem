using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.BookingAggregate.Enum
{
    public enum BookingStatus
    {
        // e.g., pending, confirmed, cancelled, completed
        Pending = 1,
        Completed = 2,
        Cancelled = 3,
    }
}
