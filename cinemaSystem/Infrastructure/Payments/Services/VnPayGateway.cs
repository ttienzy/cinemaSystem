using Application.Common.Interfaces.Services;
using Application.Settings;
using Infrastructure.Payments.Constants;
using Infrastructure.Payments.Libraries;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments.Services
{
    /// <summary>
    /// VnPay implementation of IPaymentGateway.
    /// Adapts the VnPayLibrary to the abstract IPaymentGateway interface.
    /// Future gateways (Momo, ZaloPay) will simply be new classes implementing IPaymentGateway.
    /// </summary>
    public class VnPayGateway(
        IOptions<VnPaySettings> vnPaySettings,
        IOptions<PaymentCallBackSettings> callbackSettings,
        IOptions<TimeZoneSettings> timeZoneSettings) : IPaymentGateway
    {
        private readonly VnPaySettings _settings = vnPaySettings.Value;
        private readonly PaymentCallBackSettings _callback = callbackSettings.Value;
        private readonly TimeZoneSettings _tz = timeZoneSettings.Value;

        public string CreatePaymentUrl(
            PaymentRequest request, string clientIpAddress, string baseReturnUrl)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(_tz.Id);
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var txnRef = now.Ticks.ToString();

            var pay = new VnPayLibrary();
            pay.AddRequestData("vnp_Version", _settings.Vnp_Version);
            pay.AddRequestData("vnp_Command", _settings.Vnp_Command);
            pay.AddRequestData("vnp_TmnCode", _settings.Vnp_TmnCode);
            pay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
            pay.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _settings.Vnp_CurrCode);
            pay.AddRequestData("vnp_IpAddr", clientIpAddress);
            pay.AddRequestData("vnp_Locale", _settings.Vnp_Locale);
            pay.AddRequestData("vnp_OrderInfo", $"{request.BookingCode} - {request.Description}");
            pay.AddRequestData("vnp_OrderType", "DIG");
            pay.AddRequestData("vnp_ReturnUrl", _callback.Vnp_ReturnUrl);
            pay.AddRequestData("vnp_ExpireDate",
                now.AddMinutes(PaymentConstants.ExpireInMinutes).ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_TxnRef", txnRef);

            return pay.CreateRequestUrl(_settings.BaseUrl, _settings.HashSecret);
        }

        public PaymentCallbackResult ProcessCallback(IDictionary<string, string> callbackParams)
        {
            var pay = new VnPayLibrary();
            foreach (var (key, value) in callbackParams)
                pay.AddResponseData(key, value);

            var isValid = pay.ValidateSignature(
                Get(callbackParams, "vnp_SecureHash"),
                _settings.HashSecret);

            if (!isValid)
                return new PaymentCallbackResult(false, Guid.Empty, null, null, "Invalid signature.");

            var responseCode = Get(callbackParams, "vnp_ResponseCode");
            var isSuccess = responseCode == "00";
            var bookingOrderInfo = Get(callbackParams, "vnp_OrderInfo");
            var txnId = callbackParams.TryGetValue("vnp_TransactionNo", out var txn) ? txn : null;
            var refCode = callbackParams.TryGetValue("vnp_TxnRef", out var rf) ? rf : null;

            // Extract BookingId from the order info (format: "BKxxxxxxxx - description")
            if (!Guid.TryParse(
                bookingOrderInfo.Contains(" - ")
                    ? bookingOrderInfo.Split(" - ")[0]
                    : bookingOrderInfo,
                out var bookingId))
                return new PaymentCallbackResult(false, Guid.Empty, null, null, "Cannot parse BookingId.");

            return new PaymentCallbackResult(isSuccess, bookingId, txnId, refCode,
                isSuccess ? null : $"VnPay error code: {responseCode}");
        }

        private static string Get(IDictionary<string, string> dict, string key)
            => dict.TryGetValue(key, out var value) ? value : string.Empty;

    }
}
