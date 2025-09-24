using Microsoft.AspNetCore.Http;
using Shared.Common.Base;
using Shared.Models.DataModels.BookingDtos;
using Shared.Models.PaymentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface IBookingService
    {
        Task<BaseResponse<string>> CreateBookingAsync(PaymentInfomationRequest request, HttpContext httpContext);
        Task<BaseResponse<string>> CancelPaymentAsync(PaymentResponse response);
        Task<BaseResponse<BookingCheckedInResponse>> CheckInBookingAsync(Guid bookingId);
        Task<BaseResponse<string>> ConfirmCheckedIn(Guid bookingId);
        Task<BaseResponse<string>> ConfirmPaymentAsync(PaymentResponse response);
        Task<BaseResponse<IEnumerable<PurchaseResponse>>> PurchaseHistoryAsync(Guid userId);
    }
}
