using Booking.API.Infrastructure.Hubs.Constants;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Infrastructure.Hubs.Extensions;

public static class HubCallerContextExtensions
{
    public static string GetUserNameOrAnonymous(this HubCallerContext context)
        => context.User?.Identity?.Name ?? HubConstants.AnonymousUser;
}
