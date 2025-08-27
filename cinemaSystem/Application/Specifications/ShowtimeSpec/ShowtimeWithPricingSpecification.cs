using Ardalis.Specification;
using Domain.Entities.ShowtimeAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.ShowtimeSpec
{
    public class ShowtimeWithPricingSpecification : Specification<Showtime>
    {
        public ShowtimeWithPricingSpecification(Guid showtimeId)
        {
            Query.Where(s => s.Id == showtimeId)
                 .Include(s => s.ShowtimePricings);
        }
        public ShowtimeWithPricingSpecification(Guid showtimeId, Guid pricingId)
        {
            Query.Where(s => s.Id == showtimeId && s.ShowtimePricings.Any(p => p.Id == pricingId))
                 .Include(s => s.ShowtimePricings);
        }
    }
}
