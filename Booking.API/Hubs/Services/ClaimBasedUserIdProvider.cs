using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Hubs.Services;

public class ClaimBasedUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirstValue("sub") ??
               connection.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
               connection.User?.FindFirstValue("nameid");
    }
}
