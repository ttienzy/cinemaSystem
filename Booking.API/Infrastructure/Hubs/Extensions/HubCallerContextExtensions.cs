using Booking.API.Infrastructure.Hubs.Constants;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Infrastructure.Hubs.Extensions;

public static class HubCallerContextExtensions
{
    public static string GetUserIdOrAnonymous(this HubCallerContext context)
        => string.IsNullOrWhiteSpace(context.UserIdentifier)
            ? HubConstants.AnonymousUser
            : context.UserIdentifier;
}
