using Cinema.Shared.Models;
using Payment.API.Application.DTOs.Requests;
using Payment.API.Application.DTOs.Responses;
using Payment.API.Domain.Entities;

namespace Payment.API.Application.Services;

public interface IPaymentService
{
    Task<ApiResponse<PaymentEntity>> CreatePaymentAsync(CreatePaymentRequest request);
    Task<ApiResponse<PaymentEntity>> GetPaymentByIdAsync(Guid id);
    Task<ApiResponse<PaymentEntity>> GetPaymentByBookingIdAsync(Guid bookingId);
    Task<ApiResponse<PaymentEntity>> GetPaymentByOrderInvoiceNumberAsync(string orderInvoiceNumber);
    Task<ApiResponse<PaginatedResponse<PaymentSearchItemResponse>>> SearchPaymentsAsync(string? query, int pageNumber, int pageSize);
    Task<ApiResponse<bool>> UpdatePaymentStatusAsync(
        Guid paymentId,
        PaymentStatus status,
        string? transactionId = null,
        string? paymentMethod = null,
        DateTime? completedAt = null,
        string? gatewayMetadata = null);
}


