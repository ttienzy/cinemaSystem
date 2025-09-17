using Domain.Common;
using Domain.Entities.CinemaAggreagte.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.CinemaAggreagte
{
    public class Screen : BaseEntity
    {
        public Guid CinemaId { get; private set; }
        public string ScreenName { get; private set; }
        public ScreenType Type { get; private set; }
        public ScreenStatus Status { get; private set; }


        private readonly List<Seat> _seats = new();
        public IReadOnlyCollection<Seat> Seats => _seats.AsReadOnly();

        public Screen()
        {
        }
        public Screen(Guid movieId, string screenName, ScreenType type, ScreenStatus status)
        {
            CinemaId = movieId;
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
        public List<Seat> GetAllSeats()
        {
            return _seats;
        }
        public void IsBlockSeats(List<Guid> seatIds)
        {
            foreach (var seat in _seats)
            {
                if (seatIds.Contains(seat.Id))
                {
                    seat.MarkAsBlocked();
                }
            }
        }
        public void AddItems(List<Seat> seats)
        {
            foreach (var seat in seats)
            {
                // check duplicate {rowName+Number}
                if (!_seats.Any(s => s.RowName == seat.RowName && s.Number == seat.Number))
                {
                    _seats.Add(seat);
                }
            }
        }
        public void RemoveSeats(List<Guid> seatIds)
        {
            foreach (var seatId in seatIds)
            {
                var seat = _seats.FirstOrDefault(s => s.Id == seatId);
                if (seat != null)
                {
                    _seats.Remove(seat);
                }
            }
        }
        public void UpdateItem(Guid seatId, Guid seatTypeId, string rowName, int number, bool isActive, bool isBlocked)
        {
            var seat = _seats.FirstOrDefault(s => s.Id == seatId);
            if (seat != null)
            {
                seat.UpdateDetails(seatTypeId, rowName, number, isActive, isBlocked);
            }
        }

    }
}
