namespace Payment.API.Domain.Exceptions;

public static class PaymentException
{
    public const string PAYMENT_NOT_FOUND = "Payment not found";
    public const string PAYMENT_NOT_FOUND_FOR_BOOKING = "Payment not found for booking";
    public const string PAYMENT_CREATED_SUCCESSFULLY = "Payment created successfully";
    public const string PAYMENT_STATUS_UPDATED_SUCCESSFULLY = "Payment status updated";
    public const string PAYMENT_CREATE_FAILED = "Failed to create payment";
    public const string PAYMENT_STATUS_UPDATE_FAILED = "Failed to update payment status";
}
