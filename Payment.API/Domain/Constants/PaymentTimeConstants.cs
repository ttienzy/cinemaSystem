namespace Payment.API.Domain.Constants;

public static class PaymentTimeConstants
{
    public const int PaymentExpiryMinutes = 15;
    public const int CheckoutRedirectDelayMilliseconds = 1000;
    public const string InvoiceTimestampFormat = "yyyyMMddHHmmss";
    public const string GatewayTransactionDateFormat = "yyyy-MM-dd HH:mm:ss";
}
