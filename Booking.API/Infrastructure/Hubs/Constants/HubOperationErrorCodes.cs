namespace Booking.API.Infrastructure.Hubs.Constants;

public static class HubOperationErrorCodes
{
    public const string ShowtimeNotFound = "showtime_not_found";
    public const string ShowtimeInactive = "showtime_inactive";
    public const string JoinShowtimeFailed = "join_showtime_failed";
    public const string LeaveShowtimeFailed = "leave_showtime_failed";
    public const string JoinBookingGroupFailed = "join_booking_group_failed";
    public const string LeaveBookingGroupFailed = "leave_booking_group_failed";
}
