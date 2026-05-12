namespace Booking.API.Domain.Exceptions;

public static class BookingException
{
    public const string VALIDATION_FAILED = "Validation failed";
    public const string BOOKING_CREATED_SUCCESSFULLY = "Booking created successfully";
    public const string BOOKING_CANCELLED_SUCCESSFULLY = "Booking cancelled successfully";
    public const string BOOKING_CONFIRMED_SUCCESSFULLY = "Booking confirmed successfully";
    public const string BOOKING_EXPIRED_SUCCESSFULLY = "Booking expired successfully";
    public const string BOOKING_CREATION_FAILED = "Failed to create booking due to system error";
    public const string BOOKING_CANCELLATION_FAILED = "Failed to cancel booking due to system error";
    public const string BOOKING_CONFIRMATION_FAILED = "Failed to confirm booking due to system error";
    public const string BOOKING_EXPIRATION_FAILED = "Failed to expire booking due to system error";
    public const string UNAUTHORIZED_CANCELLATION = "You can only cancel your own bookings";
    public const string SHOWTIME_NOT_AVAILABLE = "Showtime is no longer available for booking";
    public const string INVALID_SEATS_MESSAGE = "Some selected seats do not exist";
    public const string ALREADY_CANCELLED_MESSAGE = "Booking is already cancelled";
    public const string BOOKING_EXPIRED_MESSAGE = "Booking has expired";
    public const string USER_CANCELLED_REASON = "User cancelled";

    public static string BOOKING_NOT_FOUND(Guid bookingId) => $"Booking {bookingId} not found";
    public static string SHOWTIME_NOT_FOUND(Guid showtimeId) => $"Showtime {showtimeId} not found";
    public static string USER_BOOKINGS_FOUND(int count) => $"Found {count} bookings";
    public static string INVALID_CONFIRM_STATUS_MESSAGE(BookingStatus status) =>
        $"Booking is not in Pending status (current: {status})";
    public static string INVALID_EXPIRE_STATUS_MESSAGE(BookingStatus status) =>
        $"Cannot expire booking with status {status}";

    public static (string Code, string Message, string Field) SHOWTIME_INACTIVE
        => ("SHOWTIME_INACTIVE", "Showtime is no longer available", "ShowtimeId");

    public static (string Code, string Message, string Field) INVALID_SEATS
        => ("INVALID_SEATS", "Some selected seats do not exist", "SeatIds");

    public static (string Code, string Message, string Field) SEATS_REQUIRED
        => ("SEATS_REQUIRED", "At least one seat must be selected", "SeatIds");

    public static (string Code, string Message, string Field) MAX_SEATS_EXCEEDED(int maxSeats)
        => ("MAX_SEATS_EXCEEDED", $"Cannot book more than {maxSeats} seats at once", "SeatIds");

    public static (string Code, string Message, string Field) TOO_LATE_TO_BOOK(int minMinutes)
        => ("TOO_LATE_TO_BOOK", $"Cannot book seats less than {minMinutes} minutes before showtime", "ShowtimeId");

    public static (string Code, string Message, string Field) ALREADY_CANCELLED
        => ("ALREADY_CANCELLED", ALREADY_CANCELLED_MESSAGE, "status");

    public static (string Code, string Message, string Field) BOOKING_EXPIRED
        => ("BOOKING_EXPIRED", "Cannot cancel expired booking", "status");

    public static (string Code, string Message, string Field) INVALID_CONFIRM_STATUS(BookingStatus status)
        => ("INVALID_STATUS", $"Cannot confirm booking with status {status}", "status");

    public static (string Code, string Message, string Field) ONLY_PENDING_CAN_EXPIRE
        => ("INVALID_STATUS", "Only pending bookings can be expired", "status");

    public static (string Code, string Message, string Field) DATABASE_ERROR(string message)
        => ("DATABASE_ERROR", message, "database");
}
