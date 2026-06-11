using Payment.API.Client;
using Payment.API.Constants;
using Payment.API.Entities;
using ClientPaymentStatus = Payment.API.Client.PaymentStatus;
using DomainPaymentStatus = Payment.API.Entities.PaymentStatus;

namespace Payment.API.Mappers;

public static class PaymentMapper
{
    public static PaymentEntity MapToPaymentEntity(
        this CreatePaymentRequest request,
        string orderInvoiceNumber,
        DateTime createdAtUtc)
    {
        return new PaymentEntity
        {
            BookingId = request.BookingId,
            OrderInvoiceNumber = orderInvoiceNumber,
            Amount = request.Amount,
            Currency = "VND",
            OrderDescription = request.OrderDescription,
            PaymentGateway = "SePay",
            PaymentMethod = request.PaymentMethod,
            Status = DomainPaymentStatus.Pending,
            //CreatedAt = createdAtUtc,
            ExpiresAt = createdAtUtc.AddMinutes(PaymentTimeConstants.PaymentExpiryMinutes),
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            CustomerName = request.CustomerName,
            SuccessUrl = request.SuccessUrl,
            ErrorUrl = request.ErrorUrl,
            CancelUrl = request.CancelUrl
        };
    }

    public static PaymentSearchItemResponse PaymentMapToSearchItemResponse(this PaymentEntity payment)
    {
        return new PaymentSearchItemResponse
        {
            PaymentId = payment.Id,
            BookingId = payment.BookingId,
            OrderInvoiceNumber = payment.OrderInvoiceNumber,
            CustomerEmail = payment.CustomerEmail,
            CustomerPhone = payment.CustomerPhone,
            CustomerName = payment.CustomerName,
            Amount = payment.Amount,
            Status = (ClientPaymentStatus)payment.Status,
            //CreatedAt = payment.CreatedAt,
            CompletedAt = payment.CompletedAt
        };
    }
}
