using Domain.Entities.MovieAggregate.Enum;
using System;
using System.Collections.Generic;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieUpsertRequest
    {
        // Basic Movie fields
        public required string Title { get; set; }
        public required int DurationMinutes { get; set; }
        public required DateTime ReleaseDate { get; set; }
        public required string PosterUrl { get; set; }
        public string? Description { get; set; }
        public required RatingStatus RatingStatus { get; set; }
        public required string Trailer { get; set; }
        public required MovieStatus Status { get; set; }

        // NEW: Related data - all optional
        public List<CastCrewItem>? CastCrews { get; set; }
        public List<CertificationItem>? Certifications { get; set; }
        public List<CopyrightItem>? Copyrights { get; set; }
        public List<Guid>? GenreIds { get; set; }
    }

    public class CastCrewItem
    {
        public required string PersonName { get; set; }
        public required string RoleType { get; set; } // actor, director, producer, writer
    }

    public class CertificationItem
    {
        public required string CertificationBody { get; set; } // e.g., Vietnam Cinema Department
        public required string Rating { get; set; } // P, T13, T16, T18
        public required DateTime IssueDate { get; set; }
    }

    public class CopyrightItem
    {
        public required string DistributorCompany { get; set; }
        public required DateTime LicenseStartDate { get; set; }
        public required DateTime LicenseEndDate { get; set; }
        public required MovieCopyrightStatus Status { get; set; }
    }
}
