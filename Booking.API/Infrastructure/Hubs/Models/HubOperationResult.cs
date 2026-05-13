namespace Booking.API.Infrastructure.Hubs.Models;

public sealed class HubOperationResult
{
    public bool Success { get; init; }
    public string? Code { get; init; }
    public string? Message { get; init; }

    public static HubOperationResult Ok(string? message = null)
        => new()
        {
            Success = true,
            Message = message
        };

    public static HubOperationResult Fail(string code, string message)
        => new()
        {
            Success = false,
            Code = code,
            Message = message
        };
}
