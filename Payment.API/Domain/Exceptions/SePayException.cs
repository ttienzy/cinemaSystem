namespace Payment.API.Domain.Exceptions;

public static class SePayException
{
    public const string MERCHANT_ID_NOT_CONFIGURED = "SePay MerchantId is not configured.";
    public const string SECRET_KEY_NOT_CONFIGURED = "SePay SecretKey is not configured.";
    public const string INVALID_SECRET_KEY = "Invalid or missing X-Secret-Key.";
    public const string MISSING_INVOICE_NUMBER = "Missing order.order_invoice_number in SePay IPN payload.";
    public const string CHECKOUT_PREPARATION_FAILED = "Failed to prepare payment checkout";
}


