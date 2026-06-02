using Payment.API.Client;
using Payment.API.Entities;
using DomainPaymentStatus = Payment.API.Entities.PaymentStatus;

namespace Payment.API.Services;

public interface IPaymentService
{
    Task<ApiResponse<PaymentEntity>> CreatePaymentAsync(CreatePaymentRequest request);
    Task<ApiResponse<PaymentEntity>> GetPaymentByIdAsync(Guid id);
    Task<ApiResponse<PaymentEntity>> GetPaymentByBookingIdAsync(Guid bookingId);
    Task<ApiResponse<PaymentEntity>> GetPaymentByOrderInvoiceNumberAsync(string orderInvoiceNumber);
    Task<ApiResponse<PaginatedResponse<PaymentSearchItemResponse>>> SearchPaymentsAsync(string? query, int pageNumber, int pageSize);
    Task<ApiResponse<bool>> UpdatePaymentStatusAsync(
        Guid paymentId,
        DomainPaymentStatus status,
        string? transactionId = null,
        string? paymentMethod = null,
        DateTime? completedAt = null,
        string? gatewayMetadata = null);
}


