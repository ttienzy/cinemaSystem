using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.GenreSpec;
using Application.Specifications.MovieSpec;
using Ardalis.Specification.EntityFrameworkCore;
using Domain.Entities.MovieAggregate;
using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.MovieDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class MovieService : IMovieService
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Genre> _genreRepository;
        private readonly IMovieRepository _movieCustomRepository;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        public MovieService(
            IRepository<Movie> movieRepository, 
            IRepository<Genre> genreRepository,
            IMovieRepository movieCustomRepository,
            ICacheService cacheService,
            IUnitOfWork unitOfWork)
        {
            _movieRepository = movieRepository;
            _genreRepository = genreRepository;
            _movieCustomRepository = movieCustomRepository;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
        }
        public async Task<BaseResponse<MovieDetailsResponse>> GetMovieByIdAsync(Guid movieId)
        {
            try
            {
                var movieSpec = new MovieWithDetailsSpecification(movieId);
                var movie = await _movieRepository.FirstOrDefaultAsync(movieSpec);

                if (movie == null)
                {
                    return BaseResponse<MovieDetailsResponse>.Failure(Error.NotFound($"Movie with ID {movieId} not found."));
                }

                var genresSpec = new GenresWithMovieSpecification(movie.MovieGenres.Select(mg => mg.GenreId).ToList());
                var genres = await _genreRepository.ListAsync(genresSpec);

                var response = new MovieDetailsResponse
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    DurationMinutes = movie.DurationMinutes,
                    ReleaseDate = movie.ReleaseDate,
                    PosterUrl = movie.PosterUrl,
                    Description = movie.Description,
                    Genres = movie.MovieGenres.Select(mg => new MovieGenreResponse
                    {
                        Id = mg.GenreId,
                        GenreName = genres.FirstOrDefault(g => g.Id == mg.GenreId)?.GenreName ?? "Unknown"
                    }).ToList(),
                    Certifications = movie.Certifications.Select(mc => new MovieCertificationResponse
                    {
                        Id = mc.Id,
                        CertificationBody = mc.CertificationBody,
                        Rating = mc.Rating,
                    }).ToList(),
                    CastCrew = movie.CastCrew.Select(mc => new MovieCastCrewResponse
                    {
                        Id = mc.Id,
                        PersonName = mc.PersonName,
                        RoleType = mc.RoleType
                    }).ToList(),
                    Copyrights = movie.Copyrights.Select(mc => new MovieCopyrightResponse
                    {
                        Id = mc.Id,
                        DistributorCompany = mc.DistributorCompany,
                    }).ToList()
                };
                return BaseResponse<MovieDetailsResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<MovieDetailsResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<PaginatedList<MovieResponse>>> GetMoviesAsync(MovieQueryParameters parameters)
        {
            try
            {
                var query = _movieCustomRepository.GetMovies(parameters.Title, parameters.Status);
                var paginatedMovies = await PaginatedList<MovieResponse>.CreateAsync(query, parameters.PageIndex, parameters.PageSize);
                return BaseResponse<PaginatedList<MovieResponse>>.Success(paginatedMovies);
            }
            catch (Exception ex)
            {
                return BaseResponse<PaginatedList<MovieResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<MovieCastCrewResponse>>> AddCastCrewToMovie(Guid movieId, IEnumerable<MovieCastCrewRequest> requests)
        {
            try
            {
                var movieCastCrewSpec = new MovieWithCastCrewSpeccification(movieId);
                var movieWithCastCrew = await _movieRepository.FirstOrDefaultAsync(movieCastCrewSpec);
                if (movieWithCastCrew == null)
                {
                    return BaseResponse<IEnumerable<MovieCastCrewResponse>>.Failure(Error.NotFound($"Movie with ID {movieId} not found."));
                }
                
                List<MovieCastCrew> castCrewList = requests.Select(request => new MovieCastCrew(request.PersonName, request.RoleType)).ToList();
                movieWithCastCrew.AddRangeCastCrew(castCrewList);

                await _unitOfWork.Movies.UpdateAsync(movieWithCastCrew);

                var response = castCrewList.Select(mc => new MovieCastCrewResponse
                {
                    Id = mc.Id,
                    PersonName = mc.PersonName,
                    RoleType = mc.RoleType
                }).ToList();
                return BaseResponse<IEnumerable<MovieCastCrewResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<MovieCastCrewResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<MovieCertificationResponse>>> AddCertificationsToMovie(Guid movieId, IEnumerable<MovieCertificationRequest> requests)
        {
            try
            {
                var mcSpec = new MovieWithCertificationsSpecification(movieId); 
                var movieCertifications = await _movieRepository.FirstOrDefaultAsync(mcSpec);
                if (movieCertifications == null)
                {
                    return BaseResponse<IEnumerable<MovieCertificationResponse>>.Failure(Error.NotFound($"Movie with ID {movieId} not found."));
                }
                List<MovieCertification> certificationsList = requests.Select(request => new MovieCertification(request.CertificationBody, request.Rating, request.IssueDate)).ToList();
                movieCertifications.AddRangeCertifications(certificationsList);
                await _movieRepository.UpdateAsync(movieCertifications);
                var response = certificationsList.Select(mc => new MovieCertificationResponse
                {
                    Id = mc.Id,
                    CertificationBody = mc.CertificationBody,
                    Rating = mc.Rating,
                }).ToList();
                return BaseResponse<IEnumerable<MovieCertificationResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<MovieCertificationResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<MovieCopyrightResponse>>> AddCopyrightsToMovie(Guid movieId, IEnumerable<MovieCopyrightRequest> requests)
        {
            try
            {
                var movieCopyrightsSpec = new MovieWithCopyrightsSpecification(movieId);
                var movieWithCopyrights = await _movieRepository.FirstOrDefaultAsync(movieCopyrightsSpec);
                if (movieWithCopyrights == null)
                {
                    return BaseResponse<IEnumerable<MovieCopyrightResponse>>.Failure(Error.NotFound($"Movie with ID {movieId} not found."));
                }
                List<MovieCopyright> copyrightsList = requests.Select(request => new MovieCopyright(request.DistributorCompany, request.LicenseStartDate, request.LicenseEndDate, request.Status)).ToList();
                movieWithCopyrights.AddRangeCopyrights(copyrightsList);
                await _movieRepository.UpdateAsync(movieWithCopyrights);
                var response = copyrightsList.Select(mc => new MovieCopyrightResponse
                {
                    Id = mc.Id,
                    DistributorCompany = mc.DistributorCompany,
                    LicenseStartDate = mc.LicenseStartDate,
                    LicenseEndDate = mc.LicenseEndDate,
                }).ToList();
                return BaseResponse<IEnumerable<MovieCopyrightResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<MovieCopyrightResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<MovieGenreResponse>>> AddGenreToMovie(Guid movieId, IEnumerable<MovieGenreRequest> requests)
        {
            try
            {
                var movieGenreSpec = new MovieWithMGenreSpecification(movieId);
                var movieWithGenres = await _movieRepository.FirstOrDefaultAsync(movieGenreSpec);
                if (movieWithGenres == null)
                {
                    return BaseResponse<IEnumerable<MovieGenreResponse>>.Failure(Error.NotFound($"Movie with ID {movieId} not found."));
                }
                List<MovieGenre> genresList = requests.Select(request => new MovieGenre(request.GenreId)).ToList();
                movieWithGenres.AddRangeGenres(genresList);

                var genreIds = genresList.Select(g => g.GenreId).ToList();
                var genreSpec = new GenresWithMovieSpecification(genreIds);
                var genres = await _genreRepository.ListAsync(genreSpec);
                await _movieRepository.UpdateAsync(movieWithGenres);
                var response = genresList.Select(mc => new MovieGenreResponse
                {
                    Id = mc.Id,
                    GenreName = genres.FirstOrDefault(g => g.Id == mc.GenreId)?.GenreName ?? "Unknown",
                }).ToList();
                return BaseResponse<IEnumerable<MovieGenreResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<MovieGenreResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<MovieDetailsResponse>> CreateMovieAsync(MovieRequest request)
        {
            try
            {
                var movie = new Movie(request.Title, request.DurationMinutes, request.ReleaseDate, request.Status, request.PosterUrl, request.Description);
                await _movieRepository.AddAsync(movie);
                var response = new MovieDetailsResponse
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    DurationMinutes = movie.DurationMinutes,
                    ReleaseDate = movie.ReleaseDate,
                    PosterUrl = movie.PosterUrl,
                    Description = movie.Description,
                    Genres = new List<MovieGenreResponse>(),
                    Certifications = new List<MovieCertificationResponse>(),
                    CastCrew = new List<MovieCastCrewResponse>(),
                    Copyrights = new List<MovieCopyrightResponse>()
                };
                return BaseResponse<MovieDetailsResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<MovieDetailsResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteCastCrewForMovie(Guid movieId, IEnumerable<Guid> castCrewIds)
        {
            try
            {
                var movieCastCrewSpec = new MovieWithCastCrewSpeccification(movieId, castCrewIds.ToList());
                var movieWithCastCrew = await _movieRepository.FirstOrDefaultAsync(movieCastCrewSpec);
                if (movieWithCastCrew == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound($"Movie with ID {movieId} not found or no cast/crew with specified IDs."));
                }
                movieWithCastCrew.RemoveRangeCastCrew();
                await _movieRepository.UpdateAsync(movieWithCastCrew);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteCertificationsForMovie(Guid movieId, IEnumerable<Guid> certificationIds)
        {
            try
            {
                var movieCertificationsSpec = new MovieWithCertificationsSpecification(movieId, certificationIds.ToList());
                var movieWithCertifications = await _movieRepository.FirstOrDefaultAsync(movieCertificationsSpec);
                if (movieWithCertifications == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound($"Movie with ID {movieId} not found or no certifications with specified IDs."));
                }
                movieWithCertifications.RemoveRangeCertifications();
                await _movieRepository.UpdateAsync(movieWithCertifications);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteCopyrightsForMovie(Guid movieId, IEnumerable<Guid> copyrightIds)
        {
            try
            {
                var movieCopyrightsSpec = new MovieWithCopyrightsSpecification(movieId, copyrightIds.ToList());
                var movieWithCopyrights = await _movieRepository.FirstOrDefaultAsync(movieCopyrightsSpec);
                if (movieWithCopyrights == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound($"Movie with ID {movieId} not found or no copyrights with specified IDs."));
                }
                movieWithCopyrights.RemoveRangeCopyrights();
                await _movieRepository.UpdateAsync(movieWithCopyrights);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteGenreForMovie(Guid movieId, IEnumerable<Guid> genreIds)
        {
            try
            {
                var movieGenreSpec = new MovieWithMGenreSpecification(movieId, genreIds.ToList());
                var movieWithGenres = await _movieRepository.FirstOrDefaultAsync(movieGenreSpec);
                if (movieWithGenres == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound($"Movie with ID {movieId} not found or no genres with specified IDs."));
                }
                movieWithGenres.RemoveRangeGenres();
                await _movieRepository.UpdateAsync(movieWithGenres);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteMovieAsync(Guid movieId)
        {
            try
            {
                var movie = await _movieRepository.GetByIdAsync(movieId);
                if (movie == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound($"Movie with ID {movieId} not found."));
                }
                await _movieRepository.DeleteAsync(movie);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<MovieCastCrewResponse>> UpdateCastCrewForMovie(Guid movieId, Guid castCrewId, MovieCastCrewRequest request)
        {
            try
            {
                var movieCastCrewSpec = new MovieWithCastCrewSpeccification(movieId, castCrewId);
                var movieWithCastCrew = await _movieRepository.FirstOrDefaultAsync(movieCastCrewSpec);
                if (movieWithCastCrew == null)
                {
                    return BaseResponse<MovieCastCrewResponse>.Failure(Error.NotFound($"Movie with ID {movieId} or Cast/Crew with ID {castCrewId} not found."));
                }
                var result = movieWithCastCrew.UpdateCastCrew(castCrewId, request.PersonName, request.RoleType);
                if (!result)
                {
                    return BaseResponse<MovieCastCrewResponse>.Failure(Error.NotFound($"CastCrew witt ID {castCrewId} not found"));
                }
                await _movieRepository.UpdateAsync(movieWithCastCrew);

                var response = new MovieCastCrewResponse
                {
                    Id = castCrewId,
                    PersonName = request.PersonName,
                    RoleType = request.RoleType,
                };
                return BaseResponse<MovieCastCrewResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<MovieCastCrewResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<MovieCertificationResponse>> UpdateCertificationsForMovie(Guid movieId, Guid certId, MovieCertificationRequest request)
        {
            try
            {
                var certSpec = new MovieWithCertificationsSpecification(movieId, certId);
                var mcert = await _movieRepository.FirstOrDefaultAsync(certSpec);
                if (mcert is null)
                {
                    return BaseResponse<MovieCertificationResponse>.Failure(Error.NotFound($"Movie with ID {movieId} not found"));
                }
                var result = mcert.UpdateCertification(certId, request.CertificationBody, request.Rating, request.IssueDate);
                if (!result)
                {
                    return BaseResponse<MovieCertificationResponse>.Failure(Error.NotFound($"Cert with ID {certId} not found"));
                }
                await _movieRepository.UpdateAsync(mcert);

                var response = new MovieCertificationResponse
                {
                    Id = certId,
                    CertificationBody = request.CertificationBody,
                    Rating = request.Rating,
                };
                return BaseResponse<MovieCertificationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<MovieCertificationResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<MovieCopyrightResponse>> UpdateCopyrightsForMovie(Guid movieId, Guid copyrightId, MovieCopyrightRequest request)
        {
            try
            {
                var movieCopyrightsSpec = new MovieWithCopyrightsSpecification(movieId, copyrightId);
                var movieWithCopyrights = await _movieRepository.FirstOrDefaultAsync(movieCopyrightsSpec);
                if (movieWithCopyrights == null)
                {
                    return BaseResponse<MovieCopyrightResponse>.Failure(Error.NotFound($"Movie with ID {movieId} or Copyright with ID {copyrightId} not found."));
                }
                var result = movieWithCopyrights.UpdateCopyright(copyrightId, request.DistributorCompany, request.LicenseStartDate, request.LicenseEndDate, request.Status);
                if (!result)
                {
                    return BaseResponse<MovieCopyrightResponse>.Failure(Error.NotFound($"Copyright with ID {copyrightId} not found"));
                }
                await _movieRepository.UpdateAsync(movieWithCopyrights);

                var response = new MovieCopyrightResponse
                {
                    Id = copyrightId,
                    DistributorCompany = request.DistributorCompany,
                    LicenseStartDate = request.LicenseStartDate,
                    LicenseEndDate = request.LicenseEndDate,
                };
                return BaseResponse<MovieCopyrightResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<MovieCopyrightResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<MovieGenreResponse>> UpdateGenreForMovie(Guid movieId, Guid mGenreId, MovieGenreRequest request)
        {
            try
            {
                var movieGenreSpec = new MovieWithMGenreSpecification(movieId, mGenreId);
                var movieWithGenres = await _movieRepository.FirstOrDefaultAsync(movieGenreSpec);
                var genre = await _genreRepository.GetByIdAsync(request.GenreId);
                if (genre == null)
                {
                    return BaseResponse<MovieGenreResponse>.Failure(Error.NotFound($"Genre with ID {request.GenreId} not found."));
                }
                if (movieWithGenres == null)
                {
                    return BaseResponse<MovieGenreResponse>.Failure(Error.NotFound($"Movie with ID {movieId} or Genre with ID {mGenreId} not found."));
                }
                var result = movieWithGenres.UpdateMovieGenre(mGenreId, request.GenreId);
                if (!result)
                {
                    return BaseResponse<MovieGenreResponse>.Failure(Error.NotFound($"Genre with ID {mGenreId} not found"));
                }
                await _movieRepository.UpdateAsync(movieWithGenres);

                var response = new MovieGenreResponse
                {
                    Id = mGenreId,
                    GenreName = genre.GenreName,
                };
                return BaseResponse<MovieGenreResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<MovieGenreResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<MovieDetailsResponse>> UpdateMovieAsync(Guid movieId, MovieRequest request)
        {
            try
            {
                var movie = await _movieRepository.GetByIdAsync(movieId);
                if (movie == null)
                {
                    return BaseResponse<MovieDetailsResponse>.Failure(Error.NotFound($"Movie with ID {movieId} not found."));
                }
                movie.UpdateDetail(request.Title, request.DurationMinutes, request.ReleaseDate, request.Description, request.PosterUrl);
                await _movieRepository.UpdateAsync(movie);

                var response = new MovieDetailsResponse
                {
                    Id = movieId,
                    Title = movie.Title,
                    DurationMinutes = movie.DurationMinutes,
                    ReleaseDate = movie.ReleaseDate,
                    Description = movie.Description,
                    PosterUrl = movie.PosterUrl
                };
                return BaseResponse<MovieDetailsResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<MovieDetailsResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
