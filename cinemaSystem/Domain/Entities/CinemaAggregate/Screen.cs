using Domain.Common;
using Domain.Entities.CinemaAggregate.Enum;

namespace Domain.Entities.CinemaAggregate
{
    /// <summary>
    /// Screen entity — represents a cinema screen/room.
    /// Enhanced with LinkCoupleSeats() for pair-linking seats.
    /// </summary>
    public class Screen : BaseEntity
    {
        public Guid CinemaId { get; private set; }
        public string ScreenName { get; private set; } = string.Empty;
        public ScreenType Type { get; private set; }
        public ScreenStatus Status { get; private set; }

        private readonly List<Seat> _seats = [];
        public IReadOnlyCollection<Seat> Seats => _seats.AsReadOnly();

        // EF Core constructor
        private Screen() { }

        public Screen(Guid cinemaId, string screenName, ScreenType type, ScreenStatus status)
        {
            CinemaId = cinemaId;
            ScreenName = screenName;
            Type = type;
            Status = status;
        }

        public void UpdateDetails(string screenName, ScreenType type, ScreenStatus status)
        {
            ScreenName = screenName;
            Type = type;
            Status = status;
        }

        // -- Seat management ------------------------------------------
        public void AddSeats(List<Seat> seats)
        {
            foreach (var seat in seats)
            {
                if (!_seats.Any(s => s.RowName == seat.RowName && s.Number == seat.Number))
                    _seats.Add(seat);
            }
        }

        public void RemoveSeats(List<Guid> seatIds)
        {
            _seats.RemoveAll(s => seatIds.Contains(s.Id));
        }

        public void UpdateSeat(Guid seatId, Guid seatTypeId, string rowName, int number, bool isActive, bool isBlocked)
        {
            var seat = _seats.FirstOrDefault(s => s.Id == seatId);
            seat?.UpdateDetails(seatTypeId, rowName, number, isActive, isBlocked);
        }

        public void BlockSeats(List<Guid> seatIds, string reason = "Manually blocked")
        {
            foreach (var seat in _seats.Where(s => seatIds.Contains(s.Id)))
                seat.Block(reason);
        }

        public void UnblockSeats(List<Guid> seatIds)
        {
            foreach (var seat in _seats.Where(s => seatIds.Contains(s.Id)))
                seat.Unblock();
        }

        /// <summary>
        /// Link two adjacent seats as a couple pair.
        /// Both seats must exist in this screen and be in the same row.
        /// </summary>
        public void LinkCoupleSeats(string rowName, int seatNumber1, int seatNumber2)
        {
            var seat1 = _seats.FirstOrDefault(s => s.RowName == rowName && s.Number == seatNumber1)
                ?? throw new DomainException($"Seat {rowName}{seatNumber1} not found in screen {ScreenName}.");
            var seat2 = _seats.FirstOrDefault(s => s.RowName == rowName && s.Number == seatNumber2)
                ?? throw new DomainException($"Seat {rowName}{seatNumber2} not found in screen {ScreenName}.");

            if (Math.Abs(seatNumber1 - seatNumber2) != 1)
                throw new DomainException("Couple seats must be adjacent (consecutive numbers).");

            seat1.LinkWithSeat(seatNumber2);
            seat2.LinkWithSeat(seatNumber1);
        }

        /// <summary>Unlink a previously linked couple seat pair.</summary>
        public void UnlinkCoupleSeats(string rowName, int seatNumber1, int seatNumber2)
        {
            var seat1 = _seats.FirstOrDefault(s => s.RowName == rowName && s.Number == seatNumber1);
            var seat2 = _seats.FirstOrDefault(s => s.RowName == rowName && s.Number == seatNumber2);
            seat1?.UnlinkCoupleSeat();
            seat2?.UnlinkCoupleSeat();
        }

        public int BookableSeatsCount => _seats.Count(s => s.IsBookable());

        public List<Seat> GetAllSeats() => [.. _seats];

        // Legacy compatibility
        public void AddItems(List<Seat> seats) => AddSeats(seats);
        public void IsBlockSeats(List<Guid> seatIds) => BlockSeats(seatIds);
    }
}
