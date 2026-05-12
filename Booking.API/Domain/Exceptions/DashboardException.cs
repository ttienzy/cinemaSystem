namespace Booking.API.Domain.Exceptions;

public static class DashboardException
{
    public const string VALIDATION_FAILED = "Validation failed";
    public static (string Code, string Message, string Field) INVALID_UTC_OFFSET
        => ("INVALID_UTC_OFFSET", "utcOffsetMinutes must be between -720 and 840.", "utcOffsetMinutes");
}
