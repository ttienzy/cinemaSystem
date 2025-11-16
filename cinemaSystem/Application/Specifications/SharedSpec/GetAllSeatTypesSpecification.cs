using Ardalis.Specification;
using Domain.Entities.SharedAggregates;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.SharedSpec
{
    public class GetAllSeatTypesSpecification : Specification<SeatType, SeatTypeInfoDto>
    {
        public GetAllSeatTypesSpecification()
        {
            Query.AsNoTracking().Select(seatType => new SeatTypeInfoDto
            {
                SeatTypeId = seatType.Id,
                SeatTypeName = seatType.TypeName,
                Multiplier = seatType.PriceMultiplier,
            });
        }
        public GetAllSeatTypesSpecification(List<Guid> seatTypeIds)
        {
            Query.AsNoTracking()
                 .Where(seatType => seatTypeIds.Contains(seatType.Id))
                 .Select(seatType => new SeatTypeInfoDto
                 {
                     SeatTypeId = seatType.Id,
                     SeatTypeName = seatType.TypeName,
                     Multiplier = seatType.PriceMultiplier,
                 });
        }
    }
}
