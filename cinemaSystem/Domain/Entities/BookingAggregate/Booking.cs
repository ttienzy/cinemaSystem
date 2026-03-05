using Domain.Common;
using Domain.Entities.BookingAggregate.Enums;
using Domain.Events;

namespace Domain.Entities.BookingAggregate
{
    /// <summary>
    /// Booking aggregate root — encapsulates all booking state transitions.
    /// Events: BookingCreatedEvent, BookingCompletedEvent, BookingCancelledEvent,
    ///         RefundRequestedEvent, BookingRefundedEvent.
    /// </summary>
    public class Booking : BaseEntity, IAggregateRoot
    {
        // ── Core Properties ─────────────────────────────────────────
        public Guid? CustomerId { get; private set; }
        public Guid? CinemaId { get; private set; }
        public Guid ShowtimeId { get; private set; }
        public DateTime BookingTime { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public string BookingCode { get; private set; } = string.Empty;
        public string CheckInToken { get; private set; } = string.Empty;
        public int TotalTickets { get; private set; }
        public decimal TotalAmount { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public Guid? PromotionId { get; private set; }
        public BookingStatus Status { get; private set; }
        public bool IsCheckedIn { get; private set; }
        public DateTime? CheckedInAt { get; private set; }

        // ── Collections ──────────────────────────────────────────────
        private readonly List<BookingTicket> _bookingTickets = [];
        public IReadOnlyCollection<BookingTicket> BookingTickets => _bookingTickets.AsReadOnly();

        private readonly List<Payment> _payments = [];
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

        private readonly List<Refund> _refunds = [];
        public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly();

        // ── EF Core constructor ──────────────────────────────────────
        private Booking() { }

        // ── Factory Method ───────────────────────────────────────────
        /// <summary>
        /// Create a new booking and raise BookingCreatedEvent.
        /// Seat locking (Redis) will be handled by the event handler.
        /// </summary>
        public static Booking Create(
            Guid customerId,
            Guid showtimeId,
            int totalTickets,
            decimal totalAmount,
            List<BookingTicket> tickets,
            int expiryMinutes = 15)
        {
            var booking = new Booking
            {
                CustomerId = customerId,
                ShowtimeId = showtimeId,
                BookingTime = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                BookingCode = GenerateBookingCode(),
                CheckInToken = GenerateCheckInToken(),
                TotalTickets = totalTickets,
                TotalAmount = totalAmount,
                DiscountAmount = 0,
                Status = BookingStatus.Pending,
                IsCheckedIn = false
            };
            booking._bookingTickets.AddRange(tickets);

            booking.Raise(new BookingCreatedEvent(
                booking.Id, customerId, showtimeId,
                totalAmount, tickets.Select(t => t.SeatId).ToList(),
                booking.ExpiresAt));

            return booking;
        }

        /// <summary>
        /// Create a new booking at the counter by a staff member.
        /// Bypasses online expiry and marks as immediately confirmed once payment is added.
        /// </summary>
        public static Booking CreateAtCounter(
            Guid? customerId,
            Guid cinemaId,
            Guid showtimeId,
            int totalTickets,
            decimal totalAmount,
            List<BookingTicket> tickets)
        {
            var booking = new Booking
            {
                CustomerId = customerId,
                CinemaId = cinemaId,
                ShowtimeId = showtimeId,
                BookingTime = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(1), // Counter bookings don't "expire" in the typical online sense
                BookingCode = GenerateBookingCode(),
                TotalTickets = totalTickets,
                TotalAmount = totalAmount,
                DiscountAmount = 0,
                Status = BookingStatus.Pending, // Will be moved to Completed immediately by handler
                IsCheckedIn = true // Counter sales are usually checked in or ready to enter
            };
            booking._bookingTickets.AddRange(tickets);

            booking.Raise(new BookingCreatedEvent(
                booking.Id, customerId ?? Guid.Empty, showtimeId,
                totalAmount, tickets.Select(t => t.SeatId).ToList(),
                booking.ExpiresAt));

            return booking;
        }

        // ── Commands ────────────────────────────────────────────────

        public void Complete(Payment payment, string? bookingCode = null)
        {
            if (Status != BookingStatus.Pending)
                throw new DomainException("Only pending bookings can be completed.");

            Status = BookingStatus.Completed;
            _payments.Add(payment);

            if (!string.IsNullOrEmpty(bookingCode))
                BookingCode = bookingCode;

            Raise(new BookingCompletedEvent(
                Id, CustomerId!.Value, ShowtimeId,
                FinalAmount, BookingCode,
                _bookingTickets.Select(t => t.SeatId).ToList()));
        }

        public void Cancel(string reason = "User cancelled")
        {
            if (Status is BookingStatus.Completed or BookingStatus.Refunded)
                throw new DomainException("Cannot cancel a completed or refunded booking.");

            Status = BookingStatus.Cancelled;

            Raise(new BookingCancelledEvent(
                Id, CustomerId!.Value, ShowtimeId,
                reason, _bookingTickets.Select(t => t.SeatId).ToList()));
        }

        public void RequestRefund(decimal refundPercentage, decimal refundAmount, string reason)
        {
            if (Status != BookingStatus.Completed)
                throw new DomainException("Only completed bookings can be refunded.");

            if (IsCheckedIn)
                throw new DomainException("Cannot refund after check-in.");

            var refund = new Refund(Id, refundAmount, refundPercentage, reason);
            _refunds.Add(refund);

            Status = BookingStatus.PendingRefund;

            Raise(new RefundRequestedEvent(
                Id, CustomerId!.Value,
                refundAmount, refundPercentage, ShowtimeId));
        }

        public void ApproveRefund()
        {
            if (Status != BookingStatus.PendingRefund)
                throw new DomainException("No pending refund request to approve.");

            var pendingRefund = _refunds.LastOrDefault(r => !r.IsProcessed)
                ?? throw new DomainException("No unprocessed refund found.");

            pendingRefund.MarkAsProcessed();
            Status = BookingStatus.Refunded;
            DiscountAmount = pendingRefund.RefundAmount; // record how much was returned

            Raise(new BookingRefundedEvent(Id, CustomerId!.Value, pendingRefund.RefundAmount));
        }

        public void CheckIn()
        {
            if (Status != BookingStatus.Completed)
                throw new DomainException("Only completed bookings can be checked in.");
            if (IsCheckedIn)
                throw new DomainException("Booking has already been checked in.");

            IsCheckedIn = true;
            CheckedInAt = DateTime.UtcNow;
        }

        public void ApplyPromotion(Guid promotionId, decimal discountAmount)
        {
            if (Status != BookingStatus.Pending)
                throw new DomainException("Promotions can only be applied to pending bookings.");

            PromotionId = promotionId;
            DiscountAmount = discountAmount;
            // TotalAmount stays as the original price — FinalAmount is the computed result
        }

        // ── Computed Properties ──────────────────────────────────────
        public decimal FinalAmount => Math.Max(0, TotalAmount - DiscountAmount);

        public bool IsExpired =>
            Status == BookingStatus.Pending && DateTime.UtcNow > ExpiresAt;

        // ── Helpers ──────────────────────────────────────────────────
        private static string GenerateBookingCode()
        {
            var timestamp = DateTime.UtcNow.ToString("yyMMddHHmm");
            var suffix = Guid.NewGuid().ToString("N")[..4].ToUpper();
            return $"BK{timestamp}{suffix}";
        }

        private static string GenerateCheckInToken()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }

        // ── Legacy compatibility (for old code, not recommended) ─────
        public void AddTickets(List<BookingTicket> tickets) => _bookingTickets.AddRange(tickets);
        public void AddPayment(Payment payment) => _payments.Add(payment);

        /// <summary>Legacy: used by old Infrastructure services. Prefer CheckIn().</summary>
        public void MarkAsCheckedIn() => CheckIn();

        /// <summary>Legacy: used by old Infrastructure services. Sets status to Completed directly.</summary>
        public void MarkAsCompleted()
        {
            Status = BookingStatus.Completed;
        }

        /// <summary>Legacy constructor for backward compatibility with old services.</summary>
        public Booking(Guid? customerId, Guid showtimeId, int totalTickets, decimal totalAmount, List<BookingTicket> tickets, Payment payment)
        {
            CustomerId = customerId;
            ShowtimeId = showtimeId;
            BookingTime = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.AddMinutes(15);
            BookingCode = GenerateBookingCode();
            TotalTickets = totalTickets;
            TotalAmount = totalAmount;
            DiscountAmount = 0;
            Status = BookingStatus.Pending;
            IsCheckedIn = false;
            _bookingTickets.AddRange(tickets);
            _payments.Add(payment);
        }

        /// <summary>Legacy no-op: Id is auto-generated by BaseEntity. Kept for backward compat.</summary>
        public void CreateId() { }

    }
}
