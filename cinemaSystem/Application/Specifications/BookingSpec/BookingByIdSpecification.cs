using Ardalis.Specification;
using Domain.Entities.BookingAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.BookingSpec
{
    public class BookingByIdSpecification : Specification<Booking>
    {
        public BookingByIdSpecification(Guid bookingId)
        {
            Query.Where(b => b.Id == bookingId)
                .Include(b => b.BookingTickets)
                .AsNoTracking();
        }
    }
}
