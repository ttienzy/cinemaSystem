using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieDetailsResponse
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required int DurationMinutes { get; set; }
        public required DateTime ReleaseDate { get; set; }
        public required string PosterUrl { get; set; }
        public required string Description { get; set; }
        public List<MovieGenreResponse> Genres { get; set; } = new();
        public List<MovieCertificationResponse> Certifications { get; set; } = new();
        public List<MovieCopyrightResponse> Copyrights { get; set; } = new();
        public List<MovieCastCrewResponse> CastCrew { get; set; } = new();
    }
}
