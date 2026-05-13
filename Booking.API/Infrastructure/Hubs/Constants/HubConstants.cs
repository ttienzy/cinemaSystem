namespace Booking.API.Infrastructure.Hubs.Constants;

public static class HubConstants
{
    public const string AnonymousUser = "anonymous";
    public const string DashboardGroupName = "admin-dashboard";
    public const string EnableSignalRBroadcastsConfigKey = "Features:EnableSignalRBroadcasts";
    public const string RedisKeyPrefixConfigKey = "Redis:KeyPrefix";
    public const string DefaultRedisKeyPrefix = "cinema";
    public const bool EnableSignalRBroadcastsByDefault = true;

    public static readonly TimeSpan DefaultConnectionTrackingTtl = TimeSpan.FromHours(2);
}
