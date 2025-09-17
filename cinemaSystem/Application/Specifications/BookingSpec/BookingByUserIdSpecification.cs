using Ardalis.Specification;
using Domain.Entities.BookingAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.BookingSpec
{
    public class BookingByUserIdSpecification : Specification<Booking>
    {
        public BookingByUserIdSpecification(Guid userId)
        {
            Query.Where(b => b.CustomerId == userId).AsNoTracking();
        }
    }
}
