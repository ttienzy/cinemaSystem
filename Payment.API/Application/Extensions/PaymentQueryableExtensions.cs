using Microsoft.EntityFrameworkCore;

namespace Payment.API.Application.Extensions;

public static class PaymentQueryableExtensions
{
    public static IQueryable<PaymentEntity> ApplySearch(
        this IQueryable<PaymentEntity> payments,
        string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return payments;
        }

        var searchTerm = query.Trim();
        var normalizedPhone = NormalizePhone(searchTerm);
        var hasPhoneSearch = !string.IsNullOrWhiteSpace(normalizedPhone);

        return payments.Where(payment =>
            payment.OrderInvoiceNumber.Contains(searchTerm) ||
            payment.CustomerEmail.Contains(searchTerm) ||
            payment.CustomerPhone.Contains(searchTerm) ||
            (hasPhoneSearch &&
             EF.Functions.Like(
                 payment.CustomerPhone
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace("(", string.Empty)
                    .Replace(")", string.Empty),
                 $"%{normalizedPhone}%")));
    }

    private static string NormalizePhone(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }
}
