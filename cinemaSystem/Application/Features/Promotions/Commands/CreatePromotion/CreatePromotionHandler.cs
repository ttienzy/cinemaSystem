using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.PromotionAggregate;
using MediatR;

namespace Application.Features.Promotions.Commands.CreatePromotion
{
    public class CreatePromotionHandler(
        IPromotionRepository promoRepo,
        IUnitOfWork uow) : IRequestHandler<CreatePromotionCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePromotionCommand cmd, CancellationToken ct)
        {
            // Check for duplicate code
            var existing = await promoRepo.GetByCodeAsync(cmd.Code, ct);
            if (existing is not null)
                throw new ConflictException($"Promotion code '{cmd.Code}' already exists.");

            var type = cmd.Type == "Percentage"
                ? PromotionType.Percentage
                : PromotionType.FixedAmount;

            var promotion = Promotion.Create(
                cmd.Code, cmd.Name, cmd.Description,
                type, cmd.Value,
                cmd.MaxDiscountAmount, cmd.MinOrderValue,
                cmd.MaxUsageCount,
                cmd.StartDate, cmd.EndDate);

            await promoRepo.AddAsync(promotion, ct);
            await uow.SaveChangesAsync(ct);

            return promotion.Id;
        }
    }
}
