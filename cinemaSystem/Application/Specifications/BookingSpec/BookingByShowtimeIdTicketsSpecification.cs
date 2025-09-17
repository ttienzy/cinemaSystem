using Ardalis.Specification;
using Domain.Entities.BookingAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.BookingSpec
{
    public class BookingByShowtimeIdTicketsSpecification : Specification<Booking>
    {
        public BookingByShowtimeIdTicketsSpecification(Guid showtimeId)
        {
            Query.Where(b => b.ShowtimeId == showtimeId)
                .Include(b => b.BookingTickets)
                .AsNoTracking();
        }
    }
}
