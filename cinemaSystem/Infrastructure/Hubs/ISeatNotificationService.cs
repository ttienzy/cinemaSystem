using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Hubs
{
    public interface ISeatNotificationService
    {
        Task NotifySeatReservedAsync(Guid showtimeId, IEnumerable<Guid> seatIds);
        Task NotifySeatSoldAsync(Guid showtimeId, IEnumerable<Guid> seatIds);
        Task NotifySeatReleasedAsync(Guid showtimeId, IEnumerable<Guid> seatIds);
        Task NotifyBookingExpiredAsync(Guid showtimeId, IEnumerable<Guid> seatIds);
    }
}
