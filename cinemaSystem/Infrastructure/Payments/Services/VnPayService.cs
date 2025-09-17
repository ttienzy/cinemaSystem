using Application.Interfaces.Integrations;
using Application.Settings;
using Infrastructure.Payments.Constants;
using Infrastructure.Payments.Libraries;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shared.Models.PaymentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Payments.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPaySettings _vnPaySettings;
        private readonly PaymentCallBackSettings _paymentCallBackSettings;
        private readonly TimeZoneSettings _timeZoneSettings;
        public VnPayService(IOptions<VnPaySettings> vnPaySettings, IOptions<PaymentCallBackSettings> paymentCallBackSettings, IOptions<TimeZoneSettings> timeZoneSettings)
        {
            _vnPaySettings = vnPaySettings.Value ?? throw new ArgumentNullException(nameof(vnPaySettings), "VnPaySettings cannot be null");
            _paymentCallBackSettings = paymentCallBackSettings.Value ?? throw new ArgumentNullException(nameof(paymentCallBackSettings), "PaymentCallBackSettings cannot be null");
            _timeZoneSettings = timeZoneSettings.Value ?? throw new ArgumentNullException(nameof(timeZoneSettings), "TimeZoneSettings cannot be null");
        }
        public string CreatePaymentUrl(PaymentInfomationRequest request, HttpContext context)
        {
            var timeZonneId = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneSettings.Id);
            var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZonneId);
            var tick = currentTime.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallback = _paymentCallBackSettings.Vnp_ReturnUrl;

            pay.AddRequestData("vnp_Version", _vnPaySettings.Vnp_Version);
            pay.AddRequestData("vnp_Command", _vnPaySettings.Vnp_Command);
            pay.AddRequestData("vnp_TmnCode", _vnPaySettings.Vnp_TmnCode);
            pay.AddRequestData("vnp_Amount", ((int)request.SelectedSeats.Sum(s => s.Price) * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", currentTime.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _vnPaySettings.Vnp_CurrCode);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _vnPaySettings.Vnp_Locale);
            pay.AddRequestData("vnp_OrderInfo", $"{request.BookingId}");
            pay.AddRequestData("vnp_OrderType", "DIG");
            pay.AddRequestData("vnp_ReturnUrl", urlCallback);
            pay.AddRequestData("vnp_ExpireDate", currentTime.AddMinutes(PaymentConstants.ExpireInMinutes).ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_TxnRef", tick);

            var fullUrl = pay.CreateRequestUrl(_vnPaySettings.BaseUrl, _vnPaySettings.HashSecret);
            return fullUrl;
        }

        public PaymentResponse PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _vnPaySettings.HashSecret);

            return response;
        }
    }
}
