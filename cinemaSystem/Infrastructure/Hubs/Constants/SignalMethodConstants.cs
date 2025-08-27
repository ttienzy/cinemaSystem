using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Hubs.Constants
{
    public class SignalMethodConstants
    {
        public const string JoinEventGroup = "JoinEventGroup";
        public const string LeaveEventGroup = "LeaveEventGroup";
        public const string OnConnectedAsync = "OnConnectedAsync";
        public const string OnDisconnectedAsync = "OnDisconnectedAsync";
        public const string UpdateSeatReservation = "UpdateSeatReservation";
        public const string NotifySeatReleased = "NotifySeatReleased";
        public const string NotifySeatReserved = "NotifySeatReserved";
        public const string NotifySeatSold = "NotifySeatSold";
        public const string NotifyBookingFailed = "NotifyBookingFailed";
        public const string NotifyTemporaryReservationExpired = "NotifyTemporaryReservationExpired";
    }
}
