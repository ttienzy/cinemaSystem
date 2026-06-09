namespace Booking.API.Infrastructure.Caching.Helpers
{
    public class RedisHelper
    {
        public static string GetLockKey(Guid showtimeId, Guid seatId)
        {
            return $"seat-lock:{showtimeId}:{seatId}";
        }
    }
}
