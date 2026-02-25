namespace Application.Common.Interfaces.Services
{
    /// <summary>
    /// Abstraction over payment gateways (VnPay, Momo, ZaloPay, etc.).
    /// The Application layer uses this interface; Infrastructure implements it.
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>Generate a payment URL to redirect the user to.</summary>
        string CreatePaymentUrl(PaymentRequest request, string clientIpAddress, string baseReturnUrl);

        /// <summary>Process the callback query string from the payment gateway.</summary>
        PaymentCallbackResult ProcessCallback(IDictionary<string, string> callbackParams);
    }

    public record PaymentRequest(
        Guid BookingId,
        string BookingCode,
        decimal Amount,
        string Description);

    public record PaymentCallbackResult(
        bool IsSuccess,
        Guid BookingId,
        string? TransactionId,
        string? ReferenceCode,
        string? ErrorMessage);
}
