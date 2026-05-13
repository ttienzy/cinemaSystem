namespace Booking.API.Infrastructure.Hubs.Builders;

public static class RedisHubKeyBuilder
{
    public static string ForShowtimeConnections(string keyPrefix, Guid showtimeId)
        => $"{keyPrefix}:signalr:showtime:{showtimeId}:connections";

    public static string ForConnectionShowtimes(string keyPrefix, string connectionId)
        => $"{keyPrefix}:signalr:connection:{connectionId}:showtimes";
}
