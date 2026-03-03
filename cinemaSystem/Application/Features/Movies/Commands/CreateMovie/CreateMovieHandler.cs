using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Common;
using Domain.Entities.MovieAggregate;
using Domain.Entities.MovieAggregate.Enum;
using Domain.Entities.SharedAggregates;
using MediatR;
using Shared.Models.DataModels.MovieDtos;

namespace Application.Features.Movies.Commands.CreateMovie
{
    public record CreateMovieCommand(MovieUpsertRequest Request) : IRequest<Guid>;

    public class CreateMovieHandler(
        IMovieRepository movieRepo,
        IGenreRepository genreRepo,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateMovieCommand, Guid>
    {
        public async Task<Guid> Handle(CreateMovieCommand request, CancellationToken ct)
        {
            var movie = new Movie(
                request.Request.Title,
                request.Request.DurationMinutes,
                request.Request.ReleaseDate,
                request.Request.Status,
                request.Request.Description ?? string.Empty,
                request.Request.RatingStatus,
                request.Request.PosterUrl,
                request.Request.Trailer
            );

            await movieRepo.AddAsync(movie, ct);

            // Add Genres (if provided)
            if (request.Request.GenreIds?.Any() == true)
            {
                foreach (var genreId in request.Request.GenreIds)
                {
                    var genre = await genreRepo.GetByIdAsync(genreId, ct)
                        ?? throw new NotFoundException(nameof(Genre), genreId);
                    movie.AddRangeGenres([new MovieGenre(genreId, movie.Id)]);
                }
            }

            // Add CastCrew (if provided)
            if (request.Request.CastCrews?.Any() == true)
            {
                var castCrews = request.Request.CastCrews
                    .Select(c => new MovieCastCrew(movie.Id, c.PersonName, c.RoleType))
                    .ToList();
                movie.AddRangeCastCrew(castCrews);
            }

            // Add Certifications (if provided)
            if (request.Request.Certifications?.Any() == true)
            {
                var certifications = request.Request.Certifications
                    .Select(c => new MovieCertification(movie.Id, c.CertificationBody, c.Rating, c.IssueDate))
                    .ToList();
                movie.AddRangeCertifications(certifications);
            }

            // Add Copyrights (if provided)
            if (request.Request.Copyrights?.Any() == true)
            {
                foreach (var copyright in request.Request.Copyrights)
                {
                    if (copyright.LicenseEndDate <= copyright.LicenseStartDate)
                        throw new DomainException("Copyright end date must be after start date.");

                    movie.AddRangeCopyrights([new MovieCopyright(
                        movie.Id,
                        copyright.DistributorCompany,
                        copyright.LicenseStartDate,
                        copyright.LicenseEndDate,
                        copyright.Status)]);
                }
            }

            await unitOfWork.SaveChangesAsync(ct);

            return movie.Id;
        }
    }
}
