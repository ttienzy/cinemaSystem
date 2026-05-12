namespace Booking.API.Domain.Exceptions;

public static class TicketOperationException
{
    public const string NO_TICKETS_FOUND = "No tickets found";
    public const string TICKETS_FOUND = "ticket(s)";
    public const string TICKET_CHECKED_IN_SUCCESSFULLY = "Ticket checked in successfully";
    public const string PAYMENT_NOT_FOUND_FOR_BOOKING = "Payment information not found for this booking";

    public const string ALREADY_CHECKED_IN_MESSAGE = "Ticket has already been checked in";
    public static (string Code, string Message, string Field) ALREADY_CHECKED_IN
        => ("ALREADY_CHECKED_IN", ALREADY_CHECKED_IN_MESSAGE, "bookingStatus");

    public const string INVALID_BOOKING_STATUS_MESSAGE = "Only paid bookings can be checked in";
    public static (string Code, string Message, string Field) INVALID_BOOKING_STATUS(BookingStatus status)
        => ("INVALID_BOOKING_STATUS", $"Current booking status is {status}", "bookingStatus");

    public const string PAYMENT_NOT_COMPLETED_MESSAGE = "Only paid tickets can be checked in";
    public static (string Code, string Message, string Field) PAYMENT_NOT_COMPLETED(PaymentLookupStatus status)
        => ("PAYMENT_NOT_COMPLETED", $"Current payment status is {status}", "paymentStatus");
}
