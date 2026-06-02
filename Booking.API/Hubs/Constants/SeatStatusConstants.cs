#if false // Disabled during Booking refactor: Redis/SignalR/RabbitMQ integration is paused.
namespace Booking.API.Hubs.Constants;

public static class SeatStatusConstants
{
    public const string Locked = "locked";
    public const string Available = "available";
    public const string Booked = "booked";
}
#endif
