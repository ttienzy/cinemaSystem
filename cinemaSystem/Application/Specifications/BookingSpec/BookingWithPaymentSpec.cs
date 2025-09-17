using Ardalis.Specification;
using Domain.Entities.BookingAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.BookingSpec
{
    public class BookingWithPaymentSpec : Specification<Booking>
    {
        public BookingWithPaymentSpec(Guid bookingId)
        {
            Query.Where(b => b.Id == bookingId)
                 .Include(b => b.Payments)
                 .Include(b => b.BookingTickets);
        }
    }
}
