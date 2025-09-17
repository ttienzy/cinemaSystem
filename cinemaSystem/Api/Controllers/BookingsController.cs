using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg;
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
        public BookingsController(
            IBookingService bookingService,
            IVnPayService vnPayService)
        {
            _bookingService = bookingService;
            _vnPayService = vnPayService;
        }

        [Authorize]
        [HttpGet("purchase-history/{userId:guid}")]
        public async Task<IActionResult> PurchaseAsync(Guid userId)
        {
            var result = await _bookingService.PurchaseHistoryAsync(userId);
            if (result.IsSuccess) 
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<PurchaseResponse>>.WithError(result);
        }


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
                    return Redirect($"http://localhost:5173/payment/success?showtimeId={result.Value}");
                }
                return ErrorResponse<string>.WithError(result);
            }
            else
            {
                var result = await _bookingService.CancelPaymentAsync(response);
                if (result.IsSuccess)
                {
                    return Redirect($"http://localhost:5173/{result.Value}");
                }
                return ErrorResponse<string>.WithError(result);


            }
        }

        
    }
}
