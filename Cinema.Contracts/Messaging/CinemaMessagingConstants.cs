namespace Cinema.Contracts.Messaging;

public static class CinemaQueues
{
    public const string Booking = "cinema.booking";
    public const string Payment = "cinema.payment";
    public const string Notification = "cinema.notification";
}

public static class CinemaEventNames
{
    public const string BookingCreated = "cinema.events.booking-created";
    public const string BookingCancelled = "cinema.events.booking-cancelled";
    public const string BookingExpired = "cinema.events.booking-expired";
    public const string PaymentCompleted = "cinema.events.payment-completed";
    public const string PaymentFailed = "cinema.events.payment-failed";
}
