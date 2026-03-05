using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using MediatR;

namespace Application.Features.Shared.PricingTiers.Commands
{
    // === Command Records (đặt trong Application layer — không reference Api) ===

    /// <summary>Tạo bảng giá mới.</summary>
    public record CreatePricingTierCommand(string Name, decimal BasePrice, string? Description = null) : IRequest<Guid>;

    /// <summary>Cập nhật bảng giá.</summary>
    public record UpdatePricingTierCommand(Guid Id, string Name, decimal BasePrice, string? Description = null) : IRequest;

    /// <summary>Xóa bảng giá.</summary>
    public record DeletePricingTierCommand(Guid Id) : IRequest;

    // === Handlers ===

    /// <summary>Handler tạo bảng giá.</summary>
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

    /// <summary>Handler cập nhật bảng giá.</summary>
    public class UpdatePricingTierHandler(
        IPricingTierRepository repo, IUnitOfWork uow)
        : IRequestHandler<UpdatePricingTierCommand>
    {
        public async Task Handle(UpdatePricingTierCommand request, CancellationToken ct)
        {
            var tier = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy bảng giá ID: {request.Id}");
            tier.UpdatePricingTier(request.Name, request.BasePrice, request.Description);
            repo.Update(tier);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Handler xóa bảng giá.</summary>
    public class DeletePricingTierHandler(
        IPricingTierRepository repo, IUnitOfWork uow)
        : IRequestHandler<DeletePricingTierCommand>
    {
        public async Task Handle(DeletePricingTierCommand request, CancellationToken ct)
        {
            var tier = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new KeyNotFoundException($"Không tìm thấy bảng giá ID: {request.Id}");
            repo.Delete(tier);
            await uow.SaveChangesAsync(ct);
        }
    }
}
