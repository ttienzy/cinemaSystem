using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.SharedDtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Shared.SeatTypes.Queries.GetAll
{
    public record GetAllSeatTypesQuery() : IRequest<List<SeatTypeDto>>;

    public class GetAllSeatTypesHandler(ISeatTypeRepository repo) : IRequestHandler<GetAllSeatTypesQuery, List<SeatTypeDto>>
    {
        public async Task<List<SeatTypeDto>> Handle(GetAllSeatTypesQuery request, CancellationToken ct)
        {
            var items = await repo.GetAllAsync(ct);

            return items.Select(x => new SeatTypeDto
            {
                Id = x.Id,
                TypeName = x.TypeName,
                PriceMultiplier = x.PriceMultiplier
            }).ToList();
        }
    }
}
