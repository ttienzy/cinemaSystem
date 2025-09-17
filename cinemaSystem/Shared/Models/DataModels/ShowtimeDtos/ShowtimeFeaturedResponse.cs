using Domain.Entities.MovieAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimeFeaturedResponse
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PostUrl { get; set; }
        public string Trailer { get; set; }
        public List<string> Genres { get; set; }
        public int DurationMinutes { get; set; }
        public RatingStatus AgeRating { get; set; }
        public string CinemaName { get; set; }
        public DateTime ReleaseDate { get; set; }
        public IEnumerable<ScreeningSlot> ScreeningSlots { get; set; }
    }
    public class ScreeningSlot
    {
        public Guid ShowtimeId { get; set; }
        public DateTime ActualStartTime { get; set; }
        public DateTime ActualEndTime { get; set; }
    }

}
