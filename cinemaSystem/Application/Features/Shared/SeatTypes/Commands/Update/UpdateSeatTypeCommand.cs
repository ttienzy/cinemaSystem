using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.SharedAggregates;
using MediatR;
using Shared.Models.DataModels.SharedDtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Shared.SeatTypes.Commands.Update
{
    public record UpdateSeatTypeCommand(Guid Id, UpdateSeatTypeRequest Request) : IRequest;

    public class UpdateSeatTypeHandler(ISeatTypeRepository repo, IUnitOfWork unitOfWork) : IRequestHandler<UpdateSeatTypeCommand>
    {
        public async Task Handle(UpdateSeatTypeCommand request, CancellationToken ct)
        {
            var item = await repo.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(SeatType), request.Id);

            item.UpdateSeatType(request.Request.TypeName, request.Request.PriceMultiplier);
            
            repo.Update(item);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
