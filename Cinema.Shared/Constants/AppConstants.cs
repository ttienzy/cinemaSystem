namespace Cinema.Shared.Constants;

public static class AppConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";
        public const string Customer = "Customer";
    }

    public static class SeatLock
    {
        public const int ExpirationMinutes = 10;
    }

    public static class Booking
    {
        public const int PaymentTimeoutMinutes = 15;
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }
}
