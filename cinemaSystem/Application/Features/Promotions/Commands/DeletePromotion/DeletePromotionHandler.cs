using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Promotions.Commands.DeletePromotion
{
    public class DeletePromotionHandler(
        IPromotionRepository promoRepo,
        IUnitOfWork uow) : IRequestHandler<DeletePromotionCommand, bool>
    {
        public async Task<bool> Handle(DeletePromotionCommand cmd, CancellationToken ct)
        {
            var promotion = await promoRepo.GetByIdAsync(cmd.Id, ct);

            if (promotion is null)
                throw new NotFoundException("Promotion", cmd.Id);

            // Soft delete - just deactivate
            promotion.Deactivate();
            promoRepo.Update(promotion);
            await uow.SaveChangesAsync(ct);

            return true;
        }
    }
}
