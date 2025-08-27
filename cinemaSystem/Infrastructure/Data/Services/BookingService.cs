using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.BookingSpec;
using Domain.Entities.BookingAggregate;
using Microsoft.AspNetCore.Http;
using Shared.Common.Base;
using Shared.Models.PaymentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVnPayService _vnPayService;
        public BookingService(
            IUnitOfWork unitOfWork,
            IVnPayService vnPayService)
        {
            _unitOfWork = unitOfWork;
            _vnPayService = vnPayService;
        }

        public async Task<BaseResponse<string>> CancelPaymentAsync(PaymentResponse response)
        {
            try
            {
                await _unitOfWork.BeginTractionAsync();

                var bookingSpec = new BookingWithPaymentSpec(Guid.Parse(response.OrderId));
                var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(bookingSpec);
                if (booking == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("Booking not found"));
                }
                booking.MarkAsCanceled();
                var payment = booking.Payments.FirstOrDefault();
                if (payment == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("Payment not found"));
                }
                payment.UpdatePayment(response.PaymentMethod, response.TransactionId, response.VnPayResponseCode);
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.CommitTransactionAsync();
                return BaseResponse<string>.Success("Payment completed successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<BaseResponse<string>> ConfirmPaymentAsync(PaymentResponse response)
        {
            try
            {
                await _unitOfWork.BeginTractionAsync();

                var bookingSpec = new BookingWithPaymentSpec(Guid.Parse(response.OrderId));
                var booking = await _unitOfWork.Bookings.FirstOrDefaultAsync(bookingSpec);
                if (booking == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("Booking not found"));
                }
                booking.MarkAsCompleted();
                var payment = booking.Payments.FirstOrDefault();
                if (payment == null)
                {
                    return BaseResponse<string>.Failure(Error.NotFound("Payment not found"));
                }
                payment.UpdatePayment(response.PaymentMethod, response.TransactionId, response.VnPayResponseCode);
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.CommitTransactionAsync();
                return BaseResponse<string>.Success("Payment canceled successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task<BaseResponse<string>> CreateBookingAsync(PaymentInfomationRequest request, HttpContext httpContext)
        {
            try
            {
                await _unitOfWork.BeginTractionAsync();

                var newBooking = new Booking(request.UserId, request.ShowtimeId, request.SelectedSeats.Count(), request.TotalAmount);
                newBooking.AddTickets(request.SelectedSeats.Select(seat => new BookingTicket(seat.SeatId, seat.Price)).ToList());
                newBooking.AddPayment(new Payment(newBooking.TotalAmount));

                await _unitOfWork.Bookings.AddAsync(newBooking);
                request.BookingId = newBooking.Id;
                var paymentUrl = _vnPayService.CreatePaymentUrl(request, httpContext);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return BaseResponse<string>.Success(paymentUrl);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BaseResponse<string>.Failure(Error.InternalServerError(ex.Message));
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }
    }
}
