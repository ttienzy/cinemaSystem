using Domain.Common;
using Domain.Entities.BookingAggregate.Enum;
using Domain.Entities.ShowtimeAggregate;


namespace Domain.Entities.BookingAggregate
{
    public class Booking : BaseEntity, IAggregateRoot
    {
        public Guid? CustomerId { get; private set; } 
        public Guid ShowtimeId { get; private set; } 
        public DateTime BookingTime { get; private set; }
        public int TotalTickets { get; private set; }
        public decimal TotalAmount { get; private set; }
        public BookingStatus Status { get; private set; } 
        public bool IsCheckedIn { get; private set; } = false;

        private readonly List<BookingTicket> _bookingTickets = new();
        public IReadOnlyCollection<BookingTicket> BookingTickets => _bookingTickets.AsReadOnly();

        private readonly List<Payment> _payments = new();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

        public Booking()
        {
        }
        public Booking(Guid? customerId, Guid showtimeId, int totalTickets, decimal totalAmount)
        {
            CustomerId = customerId;
            ShowtimeId = showtimeId;
            BookingTime = DateTime.UtcNow;
            TotalTickets = totalTickets;
            TotalAmount = totalAmount;
            Status = BookingStatus.Pending;
            IsCheckedIn = false;
        }
        public Booking(Guid? customerId, Guid showtimeId, int totalTickets, decimal totalAmount, List<BookingTicket> tickets, Payment payment)
        {
            CustomerId = customerId;
            ShowtimeId = showtimeId;
            BookingTime = DateTime.UtcNow;
            TotalTickets = totalTickets;
            TotalAmount = totalAmount;
            Status = BookingStatus.Pending;
            IsCheckedIn = false;
            _bookingTickets.AddRange(tickets);
            _payments.Add(payment);
        }
        public void CreateId()
        {
            Id = Guid.NewGuid();
        }
        public void MarkAsCanceled()
        {
            Status = BookingStatus.Cancelled;
        }
        public void MarkAsCompleted()
        {
            Status = BookingStatus.Completed;
        }
        public void MarkAsCheckedIn()
        {
            IsCheckedIn = true;
        }

        public void AddTickets(List<BookingTicket> tickets)
        {
            _bookingTickets.AddRange(tickets);
        }
        public void AddPayment(Payment payment)
        {
            _payments.Add(payment);
        }
        public bool GetTicketAvailability(Guid seatId)
        {
            return _bookingTickets.Any(bt => bt.SeatId == seatId);
        }
    }
}
