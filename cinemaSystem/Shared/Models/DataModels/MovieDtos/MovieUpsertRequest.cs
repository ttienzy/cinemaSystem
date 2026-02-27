using Domain.Entities.MovieAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieUpsertRequest
    {
        public required string Title { get; set; }
        public required int DurationMinutes { get; set; }
        public required DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
        public required string PosterUrl { get; set; }
        public string? Description { get; set; }
        public required RatingStatus RatingStatus { get; set; }
        public required string Trailer {  get; set; }
        public required MovieStatus Status { get; set; } = MovieStatus.ComingSoon;
    }
}
