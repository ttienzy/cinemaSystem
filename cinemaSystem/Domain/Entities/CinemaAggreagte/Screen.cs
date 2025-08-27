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
            Id = Guid.NewGuid();
        }
        public Screen( string screenName, ScreenType type, ScreenStatus status)
        {
            Id = Guid.NewGuid();
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
            _seats.AddRange(seats);
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
