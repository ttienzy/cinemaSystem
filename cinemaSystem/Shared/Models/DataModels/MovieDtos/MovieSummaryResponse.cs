using Domain.Entities.MovieAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieSummaryResponse
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required string PosterUrl { get; set; }
        public required int DurationMinutes { get; set; }
        public required DateTime ReleaseDate { get; set; }
        public required string Description { get; set; }
        public List<string> Genres { get; set; }
        public string Trailer {  get; set; }
        public RatingStatus AgeRating { get; set; }
    }
}
