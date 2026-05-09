using Cinema.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Payment.API.Infrastructure.Persistence;
using Payment.API.Application.DTOs.Requests;
using Payment.API.Application.DTOs.Responses;
using Payment.API.Domain.Entities;
using System.Security.Cryptography;

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
            var orderInvoiceNumber = GenerateOrderInvoiceNumber();

            var payment = new PaymentEntity
            {
                BookingId = request.BookingId,
                OrderInvoiceNumber = orderInvoiceNumber,
                Amount = request.Amount,
                Currency = "VND",
                OrderDescription = request.OrderDescription,
                PaymentGateway = "SePay",
                PaymentMethod = request.PaymentMethod,  // ✅ Set payment method from request
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutes to complete payment
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                CustomerName = request.CustomerName,
                SuccessUrl = request.SuccessUrl,
                ErrorUrl = request.ErrorUrl,
                CancelUrl = request.CancelUrl
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment created: {PaymentId} for booking {BookingId}",
                payment.Id, payment.BookingId);

            return ApiResponse<PaymentEntity>.SuccessResponse(payment, "Payment created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for booking {BookingId}", request.BookingId);
            return ApiResponse<PaymentEntity>.FailureResponse("Failed to create payment");
        }
    }

    public async Task<ApiResponse<PaymentEntity>> GetPaymentByIdAsync(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);

        if (payment == null)
        {
            return ApiResponse<PaymentEntity>.FailureResponse("Payment not found", 404);
        }

        return ApiResponse<PaymentEntity>.SuccessResponse(payment);
    }

    public async Task<ApiResponse<PaymentEntity>> GetPaymentByBookingIdAsync(Guid bookingId)
    {
        var payment = await _context.Payments
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        if (payment == null)
        {
            return ApiResponse<PaymentEntity>.FailureResponse("Payment not found for booking", 404);
        }

        return ApiResponse<PaymentEntity>.SuccessResponse(payment);
    }

    public async Task<ApiResponse<PaymentEntity>> GetPaymentByOrderInvoiceNumberAsync(string orderInvoiceNumber)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderInvoiceNumber == orderInvoiceNumber);

        if (payment == null)
        {
            return ApiResponse<PaymentEntity>.FailureResponse("Payment not found", 404);
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

        IQueryable<PaymentEntity> payments = _context.Payments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchTerm = query.Trim();
            var normalizedPhone = NormalizePhone(searchTerm);
            var hasPhoneSearch = !string.IsNullOrWhiteSpace(normalizedPhone);

            payments = payments.Where(p =>
                p.OrderInvoiceNumber.Contains(searchTerm) ||
                p.CustomerEmail.Contains(searchTerm) ||
                p.CustomerPhone.Contains(searchTerm) ||
                (hasPhoneSearch &&
                 p.CustomerPhone
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace("(", string.Empty)
                    .Replace(")", string.Empty)
                    .Contains(normalizedPhone)));
        }

        var totalCount = await payments.CountAsync();

        var items = await payments
            .OrderByDescending(p => p.CompletedAt ?? p.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(p => new PaymentSearchItemResponse
            {
                PaymentId = p.Id,
                BookingId = p.BookingId,
                OrderInvoiceNumber = p.OrderInvoiceNumber,
                CustomerEmail = p.CustomerEmail,
                CustomerPhone = p.CustomerPhone,
                CustomerName = p.CustomerName,
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                CompletedAt = p.CompletedAt
            })
            .ToListAsync();

        var response = PaginatedResponse<PaymentSearchItemResponse>.Create(
            items,
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
                return ApiResponse<bool>.FailureResponse("Payment not found", 404);
            }

            payment.Status = status;

            if (!string.IsNullOrEmpty(transactionId))
            {
                payment.TransactionId = transactionId;
            }

            if (!string.IsNullOrEmpty(paymentMethod))
            {
                payment.PaymentMethod = paymentMethod;
            }

            if (completedAt.HasValue)
            {
                payment.CompletedAt = completedAt.Value;
            }

            if (!string.IsNullOrWhiteSpace(gatewayMetadata))
            {
                payment.GatewayMetadata = gatewayMetadata;
            }

            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment {PaymentId} status updated to {Status}",
                paymentId, status);

            return ApiResponse<bool>.SuccessResponse(true, "Payment status updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", paymentId);
            return ApiResponse<bool>.FailureResponse("Failed to update payment status");
        }
    }

    private static string GenerateOrderInvoiceNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var suffix = RandomNumberGenerator.GetInt32(100000, 999999);
        return $"INV-{timestamp}-{suffix}";
    }

    private static string NormalizePhone(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }
}


