using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.PromotionAggregate;
using MediatR;
using Shared.Models.DataModels.PromotionDtos;

namespace Application.Features.Promotions.Commands.UpdatePromotion
{
    public class UpdatePromotionHandler(
        IPromotionRepository promoRepo,
        IMovieRepository movieRepo,
        ICinemaRepository cinemaRepo,
        IUnitOfWork uow) : IRequestHandler<UpdatePromotionCommand, PromotionResponse>
    {
        public async Task<PromotionResponse> Handle(UpdatePromotionCommand cmd, CancellationToken ct)
        {
            var request = cmd.Request;
            var promotion = await promoRepo.GetByIdAsync(cmd.Id, ct);

            if (promotion is null)
                throw new NotFoundException("Promotion", cmd.Id);

            // Check for duplicate code (excluding current promotion)
            var existingByCode = await promoRepo.GetByCodeAsync(request.Code, ct);
            if (existingByCode is not null && existingByCode.Id != cmd.Id)
                throw new ConflictException($"Promotion code '{request.Code}' already exists.");

            // Note: Code cannot be changed during update - must match existing
            if (!string.Equals(promotion.Code, request.Code, StringComparison.OrdinalIgnoreCase))
                throw new ConflictException("Promotion code cannot be changed.");

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

            promotion.UpdateDetails(
                request.Name,
                request.Description,
                type,
                request.Value,
                request.MaxDiscountAmount,
                request.MinOrderValue,
                request.MaxUsageCount,
                request.MaxUsagePerUser,
                request.StartDate,
                request.EndDate,
                request.SpecificMovieId,
                request.SpecificCinemaId);

            promoRepo.Update(promotion);
            await uow.SaveChangesAsync(ct);

            return MapToResponse(promotion);
        }

        private static PromotionResponse MapToResponse(Promotion promotion)
        {
            return new PromotionResponse
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Name = promotion.Name,
                Description = promotion.Description,
                Type = promotion.Type == PromotionType.Percentage ? "Percentage" : "FixedAmount",
                Value = promotion.Value,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                MinOrderValue = promotion.MinOrderValue,
                MaxUsageCount = promotion.MaxUsageCount,
                CurrentUsageCount = promotion.CurrentUsageCount,
                MaxUsagePerUser = promotion.MaxUsagePerUser,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive,
                SpecificMovieId = promotion.SpecificMovieId,
                SpecificCinemaId = promotion.SpecificCinemaId,
                CreatedAt = promotion.CreatedAt,
                UpdatedAt = promotion.UpdatedAt
            };
        }
    }
}
