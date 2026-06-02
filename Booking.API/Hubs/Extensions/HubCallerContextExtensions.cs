#if false // Disabled during Booking refactor: Redis/SignalR/RabbitMQ integration is paused.
using Booking.API.Hubs.Constants;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Hubs.Extensions;

public static class HubCallerContextExtensions
{
    public static string GetUserIdOrAnonymous(this HubCallerContext context)
        => string.IsNullOrWhiteSpace(context.UserIdentifier)
            ? HubConstants.AnonymousUser
            : context.UserIdentifier;
}
#endif
