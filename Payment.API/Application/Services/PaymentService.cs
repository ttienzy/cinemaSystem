using System.Security.Cryptography;
using Cinema.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Payment.API.Infrastructure.Persistence;

namespace Payment.API.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        PaymentDbContext context,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PaymentEntity>> CreatePaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            var payment = request.MapToPaymentEntity(
                GenerateOrderInvoiceNumber(),
                DateTime.UtcNow);

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment created: {PaymentId} for booking {BookingId}",
                payment.Id,
                payment.BookingId);

            return ApiResponse<PaymentEntity>.SuccessResponse(
                payment,
                PaymentException.PAYMENT_CREATED_SUCCESSFULLY,
                201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for booking {BookingId}", request.BookingId);
            return ApiResponse<PaymentEntity>.InternalServerErrorResponse(PaymentException.PAYMENT_CREATE_FAILED);
        }
    }

    public async Task<ApiResponse<PaymentEntity>> GetPaymentByIdAsync(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);

        if (payment == null)
        {
            return ApiResponse<PaymentEntity>.NotFoundResponse(PaymentException.PAYMENT_NOT_FOUND);
        }

        return ApiResponse<PaymentEntity>.SuccessResponse(payment);
    }

    public async Task<ApiResponse<PaymentEntity>> GetPaymentByBookingIdAsync(Guid bookingId)
    {
        var payment = await _context.Payments
            .OrderByDescending(payment => payment.CreatedAt)
            .FirstOrDefaultAsync(payment => payment.BookingId == bookingId);

        if (payment == null)
        {
            return ApiResponse<PaymentEntity>.NotFoundResponse(PaymentException.PAYMENT_NOT_FOUND_FOR_BOOKING);
        }

        return ApiResponse<PaymentEntity>.SuccessResponse(payment);
    }

    public async Task<ApiResponse<PaymentEntity>> GetPaymentByOrderInvoiceNumberAsync(string orderInvoiceNumber)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(payment => payment.OrderInvoiceNumber == orderInvoiceNumber);

        if (payment == null)
        {
            return ApiResponse<PaymentEntity>.NotFoundResponse(PaymentException.PAYMENT_NOT_FOUND);
        }

        return ApiResponse<PaymentEntity>.SuccessResponse(payment);
    }

    public async Task<ApiResponse<PaginatedResponse<PaymentSearchItemResponse>>> SearchPaymentsAsync(
        string? query,
        int pageNumber,
        int pageSize)
    {
        var safePage = Math.Max(1, pageNumber);
        var safeSize = Math.Clamp(pageSize, 1, 100);

        var payments = _context.Payments
            .AsNoTracking()
            .ApplySearch(query);

        var totalCount = await payments.CountAsync();

        var items = await payments
            .OrderByDescending(payment => payment.CompletedAt ?? payment.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .ToListAsync();

        var response = PaginatedResponse<PaymentSearchItemResponse>.Create(
            items.Select(payment => payment.PaymentMapToSearchItemResponse()).ToList(),
            totalCount,
            safePage,
            safeSize);

        return ApiResponse<PaginatedResponse<PaymentSearchItemResponse>>.SuccessResponse(
            response,
            $"Found {totalCount} payment record(s)");
    }

    public async Task<ApiResponse<bool>> UpdatePaymentStatusAsync(
        Guid paymentId,
        PaymentStatus status,
        string? transactionId = null,
        string? paymentMethod = null,
        DateTime? completedAt = null,
        string? gatewayMetadata = null)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(paymentId);

            if (payment == null)
            {
                return ApiResponse<bool>.NotFoundResponse(PaymentException.PAYMENT_NOT_FOUND);
            }

            payment.ApplyStatusUpdate(
                status,
                transactionId,
                paymentMethod,
                completedAt,
                gatewayMetadata);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment {PaymentId} status updated to {Status}",
                paymentId,
                status);

            return ApiResponse<bool>.SuccessResponse(
                true,
                PaymentException.PAYMENT_STATUS_UPDATED_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", paymentId);
            return ApiResponse<bool>.InternalServerErrorResponse(PaymentException.PAYMENT_STATUS_UPDATE_FAILED);
        }
    }

    private static string GenerateOrderInvoiceNumber()
    {
        var timestamp = DateTime.UtcNow.ToString(PaymentTimeConstants.InvoiceTimestampFormat);
        var suffix = RandomNumberGenerator.GetInt32(100000, 999999);
        return $"INV-{timestamp}-{suffix}";
    }
}
