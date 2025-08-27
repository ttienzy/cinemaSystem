using Domain.Common;
using Domain.Entities.MovieAggregate;
using Domain.Entities.SharedAggregates;
using Domain.Entities.ShowtimeAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.ShowtimeAggregate
{
    public class Showtime : BaseEntity, IAggregateRoot
    {
        public Guid CinemaId { get; private set; }
        public Guid MovieId { get; private set; } 
        public Guid ScreenId { get; private set; }
        public Guid SlotId { get; private set; }
        public Guid PricingTierId { get; private set; } 
        public DateTime ShowDate { get; private set; }
        public DateTime ActualStartTime { get; private set; }
        public DateTime ActualEndTime { get; private set; }
        public ShowtimeStatus Status { get; private set; } 


        private readonly List<ShowtimePricing> _showtimePricings = new();
        public IReadOnlyCollection<ShowtimePricing> ShowtimePricings => _showtimePricings.AsReadOnly();

        public Showtime()
        {
            Id = Guid.NewGuid();
        }
        public Showtime(Guid cinemaId, Guid movieId, Guid screenId, Guid slotId, Guid pricingTierId, DateTime showDate, DateTime actualStartTime, DateTime actualEndTime, ShowtimeStatus status)
        {
            Id = Guid.NewGuid();
            CinemaId = cinemaId;
            MovieId = movieId;
            ScreenId = screenId;
            SlotId = slotId;
            PricingTierId = pricingTierId;
            ShowDate = showDate;
            ActualStartTime = actualStartTime;
            ActualEndTime = actualEndTime;
            Status = status;
        }

        public void UpdateShowtime(Guid cinemaId, Guid movieId, Guid screenId, Guid slotId, Guid pricingTierId, DateTime showDate, DateTime actualStartTime, DateTime actualEndTime, ShowtimeStatus status)
        {
            CinemaId = cinemaId;
            MovieId = movieId;
            ScreenId = screenId;
            SlotId = slotId;
            PricingTierId = pricingTierId;
            ShowDate = showDate;
            ActualStartTime = actualStartTime;
            ActualEndTime = actualEndTime;
            Status = status;
        }

        public List<ShowtimePricing> GetAllShowtimePricings()
        {
            return _showtimePricings;
        }

        public void AddShowtimePricing(ShowtimePricing pricing)
        {
            _showtimePricings.Add(pricing);
        }
        public void AddRangeShowtimePricing(IEnumerable<ShowtimePricing> pricings)
        {
            _showtimePricings.AddRange(pricings);
        }
        public ShowtimePricing? GetShowtimePricingBySeatTypeId(Guid seatTypeId)
        {
            return _showtimePricings.FirstOrDefault(p => p.SeatTypeId == seatTypeId);
        }
        public bool RemoveShowtimePricing(Guid pricingId)
        {
            var pricing = GetShowtimePricingBySeatTypeId(pricingId);
            if (pricing != null)
            {
                _showtimePricings.Remove(pricing);
                return true;
            }
            return false;
        }

    }
}
