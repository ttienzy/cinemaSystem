using MediatR;

namespace Application.Features.Promotions.Commands.CreatePromotion
{
    public record CreatePromotionCommand(
        string Code,
        string Name,
        string? Description,
        string Type,    // "Percentage" or "FixedAmount"
        decimal Value,
        decimal? MaxDiscountAmount,
        decimal? MinOrderValue,
        int MaxUsageCount,
        DateTime StartDate,
        DateTime EndDate) : IRequest<Guid>;
}
