using Domain.Common;
using Domain.Entities.CinemaAggreagte;


namespace Domain.Entities.BookingAggregate
{
    public class BookingTicket: BaseEntity
    {
        public Guid SeatId { get; private set; } 
        public Guid BookingId { get; private set; }
        public decimal TicketPrice { get; private set; }
        public Seat? Seat { get; private set; }

        public BookingTicket()
        {
            Id = Guid.NewGuid();
        }
        public BookingTicket(Guid seatId, decimal ticketPrice)
        {
            Id = Guid.NewGuid();
            SeatId = seatId;
            TicketPrice = ticketPrice;
        }
    }
}
