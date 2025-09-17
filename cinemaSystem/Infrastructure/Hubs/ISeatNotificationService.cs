using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Hubs
{
    public interface ISeatNotificationService
    {
        Task NotifySeatReservedAsync(string showtimeId, string[] seatIds);
        Task NotifySeatSoldAsync(string showtimeId, string[] seatIds);
        Task NotifySeatReleasedAsync(string showtimeId, string[] seatIds);
    }
}
