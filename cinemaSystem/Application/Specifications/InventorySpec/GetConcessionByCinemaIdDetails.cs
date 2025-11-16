using Ardalis.Specification;
using Domain.Entities.InventoryAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.InventorySpec
{
    public class GetConcessionByCinemaIdDetails : Specification<InventoryItem>
    {
        public GetConcessionByCinemaIdDetails(Guid cinemaId)
        {
            Query.AsNoTracking()
                .Where(i => i.CinemaId == cinemaId);
        }
    }
}
