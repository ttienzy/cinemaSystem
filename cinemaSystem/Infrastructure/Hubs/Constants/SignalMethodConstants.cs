using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Hubs.Constants
{
    public class SignalMethodConstants
    {
        public const string JoinShowtimeGroup = "ConnectToShowtime";
        public const string LeaveShowtimeGroup = "DisconnectFromShowtime";
        //public const string OnConnectedAsync = "OnConnectedAsync";
        //public const string OnDisconnectedAsync = "OnDisconnectedAsync";
        public const string OnSeatsReleased = "OnSeatsReleased";
        public const string OnSeatsBooked = "OnSeatsBooked";
        public const string OnSeatsReserved = "OnSeatsReserved";
        public const string OnPaymentSuccessful = "OnPaymentSuccessful";
        public const string OnPaymentCanceled = "OnPaymentCanceled";
    }
}
