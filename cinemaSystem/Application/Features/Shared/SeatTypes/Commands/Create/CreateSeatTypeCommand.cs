using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using MediatR;
using Shared.Models.DataModels.SharedDtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Shared.SeatTypes.Commands.Create
{
    public record CreateSeatTypeCommand(CreateSeatTypeRequest Request) : IRequest<Guid>;

    public class CreateSeatTypeHandler(ISeatTypeRepository repo, IUnitOfWork unitOfWork) : IRequestHandler<CreateSeatTypeCommand, Guid>
    {
        public async Task<Guid> Handle(CreateSeatTypeCommand request, CancellationToken ct)
        {
            var item = new SeatType(request.Request.TypeName, request.Request.PriceMultiplier);
            
            await repo.AddAsync(item, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return item.Id;
        }
    }
}
