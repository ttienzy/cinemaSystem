using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.PromotionAggregate;
using MediatR;
using Shared.Models.DataModels.PromotionDtos;

namespace Application.Features.Promotions.Commands.CreatePromotion
{
    public class CreatePromotionHandler(
        IPromotionRepository promoRepo,
        IMovieRepository movieRepo,
        ICinemaRepository cinemaRepo,
        IUnitOfWork uow) : IRequestHandler<CreatePromotionCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePromotionCommand cmd, CancellationToken ct)
        {
            var request = cmd.Request;

            // Check for duplicate code
            var existing = await promoRepo.GetByCodeAsync(request.Code, ct);
            if (existing is not null)
                throw new ConflictException($"Promotion code '{request.Code}' already exists.");

            // Validate specific movie if provided
            if (request.SpecificMovieId.HasValue)
            {
                var movie = await movieRepo.GetByIdAsync(request.SpecificMovieId.Value, ct);
                if (movie is null)
                    throw new NotFoundException("Movie", request.SpecificMovieId.Value);
            }

            // Validate specific cinema if provided
            if (request.SpecificCinemaId.HasValue)
            {
                var cinema = await cinemaRepo.GetByIdAsync(request.SpecificCinemaId.Value, ct);
                if (cinema is null)
                    throw new NotFoundException("Cinema", request.SpecificCinemaId.Value);
            }

            var type = request.Type == "Percentage"
                ? PromotionType.Percentage
                : PromotionType.FixedAmount;

            var promotion = Promotion.Create(
                request.Code, request.Name, request.Description,
                type, request.Value,
                request.MaxDiscountAmount, request.MinOrderValue,
                request.MaxUsageCount, request.MaxUsagePerUser,
                request.StartDate, request.EndDate,
                request.SpecificMovieId, request.SpecificCinemaId);

            await promoRepo.AddAsync(promotion, ct);
            await uow.SaveChangesAsync(ct);

            return promotion.Id;
        }
    }
}
