using Ardalis.Specification;
using Domain.Entities.BookingAggregate;
using Domain.Entities.BookingAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.BookingSpec
{
    public class BookingByShowtimeIdSpecification : Specification<Booking>
    {
        public BookingByShowtimeIdSpecification()
        {
            Query.Where(b => b.Status == BookingStatus.Pending)
                .Include(bk => bk.BookingTickets)
                .Include(p => p.Payments);
        }
        public BookingByShowtimeIdSpecification(Guid showtimeId)
        {
            Query.Where(b => b.ShowtimeId == showtimeId && b.Status != BookingStatus.Cancelled).AsNoTracking();
        }
    }
}
