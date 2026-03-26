using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using MediatR;

namespace Application.Features.Shared.PricingTiers.Commands
{
    // === Command Records (placed in Application layer — no Api reference) ===

    /// <summary>Create a new pricing tier.</summary>
    public record CreatePricingTierCommand(string Name, decimal BasePrice, string? Description = null) : IRequest<Guid>;

    /// <summary>Update pricing tier.</summary>
    public record UpdatePricingTierCommand(Guid Id, string Name, decimal BasePrice, string? Description = null) : IRequest;

    /// <summary>Delete pricing tier.</summary>
    public record DeletePricingTierCommand(Guid Id) : IRequest;

    // === Handlers ===

    /// <summary>Handler for creating pricing tiers.</summary>
    public class CreatePricingTierHandler(
        IPricingTierRepository repo, IUnitOfWork uow)
        : IRequestHandler<CreatePricingTierCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePricingTierCommand request, CancellationToken ct)
        {
            var tier = new PricingTier(request.Name, request.BasePrice, request.Description);
            await repo.AddAsync(tier, ct);
            await uow.SaveChangesAsync(ct);
            return tier.Id;
        }
    }

    /// <summary>Handler for updating pricing tiers.</summary>
    public class UpdatePricingTierHandler(
        IPricingTierRepository repo, IUnitOfWork uow)
        : IRequestHandler<UpdatePricingTierCommand>
    {
        public async Task Handle(UpdatePricingTierCommand request, CancellationToken ct)
        {
            var tier = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Pricing tier not found with ID: {request.Id}");
            tier.UpdatePricingTier(request.Name, request.BasePrice, request.Description);
            repo.Update(tier);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Handler for deleting pricing tiers.</summary>
    public class DeletePricingTierHandler(
        IPricingTierRepository repo, IUnitOfWork uow)
        : IRequestHandler<DeletePricingTierCommand>
    {
        public async Task Handle(DeletePricingTierCommand request, CancellationToken ct)
        {
            var tier = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Pricing tier not found with ID: {request.Id}");
            repo.Delete(tier);
            await uow.SaveChangesAsync(ct);
        }
    }
}
