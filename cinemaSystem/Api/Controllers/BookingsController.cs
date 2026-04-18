using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Application.Settings;
using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Common.Base;
using Shared.Models.DataModels.BookingDtos;
using Shared.Models.PaymentModels;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IVnPayService _vnPayService;
        private readonly FrontendSettings _frontendSettings;
        public BookingsController(
            IBookingService bookingService,
            IVnPayService vnPayService,
            IOptions<FrontendSettings> frontendSettings)
        {
            _bookingService = bookingService;
            _vnPayService = vnPayService;
            _frontendSettings = frontendSettings.Value;
        }

        [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Manager},{RoleConstant.Employee},{RoleConstant.User}")]
        [HttpGet("purchase-history/{userId:guid}")]
        public async Task<IActionResult> PurchaseAsync(Guid userId)
        {
            var result = await _bookingService.PurchaseHistoryAsync(userId);
            if (result.IsSuccess) 
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<PurchaseResponse>>.WithError(result);
        }
        [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Manager},{RoleConstant.Employee}")]
        [HttpGet("check-in/{bookingId:guid}")]
        public async Task<IActionResult> CheckInAsync(Guid bookingId)
        {
            var result = await _bookingService.CheckInBookingAsync(bookingId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<BookingCheckedInResponse>.WithError(result);
        }
        [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Manager},{RoleConstant.Employee}")]
        [HttpPost("confirm-check-in/{bookingId:guid}")]
        public async Task<IActionResult> ConfirmCheckInAsync(Guid bookingId)
        {
            var result = await _bookingService.ConfirmCheckedIn(bookingId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<string>.WithError(result);
        }
        [Authorize(Roles = $"{RoleConstant.User}")]
        [HttpPost("create-booking")]
        public async Task<IActionResult> CreateBooking([FromBody] PaymentInfomationRequest request)
        {
            var result = await _bookingService.CreateBookingAsync(request, HttpContext);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return ErrorResponse<string>.WithError(result);
        }
        [HttpGet("callback")]
        public async Task<IActionResult> ConfirmPayment()
        {
            var query = HttpContext.Request.Query;
            var response = _vnPayService.PaymentExecute(query);
            if (response.VnPayResponseCode == "00")
            {
                var result = await _bookingService.ConfirmPaymentAsync(response);
                if (result.IsSuccess)
                {
                    return Redirect($"{_frontendSettings.BaseUrl}/payment/success?showtimeId={result.Value}");
                }
                return ErrorResponse<string>.WithError(result);
            }
            else
            {
                var result = await _bookingService.CancelPaymentAsync(response);
                if (result.IsSuccess)
                {
                    return Redirect($"{_frontendSettings.BaseUrl}/{result.Value}");
                }
                return ErrorResponse<string>.WithError(result);
            }
        }
    }
}
