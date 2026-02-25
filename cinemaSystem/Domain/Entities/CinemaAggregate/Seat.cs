using Domain.Common;
using Domain.Entities.SharedAggregates;

namespace Domain.Entities.CinemaAggregate
{
    /// <summary>
    /// Seat entity enhanced with blocking reasons and couple-seat linking support.
    /// </summary>
    public class Seat : BaseEntity
    {
        public Guid ScreenId { get; private set; }
        public Guid SeatTypeId { get; private set; }
        public string RowName { get; private set; } = string.Empty;
        public int Number { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsBlocked { get; private set; }

        // -- New fields ------------------------------------------------
        /// <summary>Reason why seat is blocked (e.g., "Maintenance", "VIP reserved")</summary>
        public string? BlockReason { get; private set; }

        /// <summary>Seat number of the linked partner (for couple/double seats)</summary>
        public int? LinkedSeatNumber { get; private set; }

        // EF Core constructor
        private Seat() { }

        public Seat(Guid seatTypeId, string rowName, int number, bool isActive, bool isBlocked, Guid screenId)
        {
            SeatTypeId = seatTypeId;
            RowName = rowName;
            Number = number;
            IsActive = isActive;
            IsBlocked = isBlocked;
            ScreenId = screenId;
        }

        // -- Computed -------------------------------------------------
        public string SeatLabel => $"{RowName}{Number}";
        public bool IsCoupleSeat => LinkedSeatNumber.HasValue;

        /// <summary>Returns true if this seat can be selected for a new booking.</summary>
        public bool IsBookable() => IsActive && !IsBlocked;

        // -- Commands -------------------------------------------------
        public void Block(string reason)
        {
            IsBlocked = true;
            BlockReason = reason;
        }

        public void Unblock()
        {
            IsBlocked = false;
            BlockReason = null;
        }

        /// <summary>Link this seat with another seat number to form a couple seat pair.</summary>
        public void LinkWithSeat(int partnerSeatNumber)
        {
            if (partnerSeatNumber == Number)
                throw new DomainException("A seat cannot be linked to itself.");

            LinkedSeatNumber = partnerSeatNumber;
        }

        public void UnlinkCoupleSeat()
        {
            LinkedSeatNumber = null;
        }

        public void UpdateDetails(Guid seatTypeId, string rowName, int number, bool isActive, bool isBlocked)
        {
            SeatTypeId = seatTypeId;
            RowName = rowName;
            Number = number;
            IsActive = isActive;
            IsBlocked = isBlocked;
        }

        // Legacy compatibility
        public void MarkAsBlocked() => Block("Manually blocked");
    }
}
