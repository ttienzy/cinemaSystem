using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.CinemaSpec;
using Application.Specifications.GenreSpec;
using Application.Specifications.MovieSpec;
using Application.Specifications.ShowtimeSpec;
using Ardalis.Specification.EntityFrameworkCore;
using Domain.Entities.CinemaAggreagte;
using Domain.Entities.MovieAggregate;
using Domain.Entities.SharedAggregates;
using Domain.Entities.ShowtimeAggregate;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.CinemaDtos;
using Shared.Models.DataModels.MovieDtos;
using Shared.Models.DataModels.StatisticDto;


namespace Infrastructure.Data.Services
{
    public class MovieService : IMovieService
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Genre> _genreRepository;
        private readonly IMovieRepository _movieCustomRepository;
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<Showtime> _showtimeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public MovieService(
            IRepository<Movie> movieRepository, 
            IRepository<Genre> genreRepository,
            IMovieRepository movieCustomRepository,
            IRepository<Cinema> cinemaRepository,
            IRepository<Showtime> showtimeRepository,
            UserManager<ApplicationUser> userManager)
        {
            _movieRepository = movieRepository;
            _genreRepository = genreRepository;
            _movieCustomRepository = movieCustomRepository;
            _cinemaRepository = cinemaRepository;
            _showtimeRepository = showtimeRepository;
            _userManager = userManager;
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
                    Movie = new MovieDetail
                    {
                        Title = movie.Title,
                        Description = movie.Description,
                        PosterUrl = movie.PosterUrl,
                        DurationMinutes = movie.DurationMinutes,
                        ReleaseDate = movie.ReleaseDate,
                        CreatedAt = movie.CreatedAt,
                    },
                    Genres = genres,
                    
                    Certifications = movie.Certifications.Select(mc => new MovieCertificationResponse
                    {
                        Id = mc.Id,
                        CertificationBody = mc.CertificationBody,
                        Rating = mc.Rating,
                        IssueDate = mc.IssueDate,
                    }).ToList(),
                    CastCrews = movie.CastCrew.Select(mc => new MovieCastCrewResponse
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
                List<MovieCastCrew> castCrewList = requests.Select(request => new MovieCastCrew(movieId, request.PersonName, request.RoleType)).ToList();
                var movieCastCrewSpec = new MovieWithCastCrewSpeccification(movieId);
                var movieWithCastCrew = await _movieRepository.FirstOrDefaultAsync(movieCastCrewSpec);
                if (movieWithCastCrew == null)
                {
                    return BaseResponse<IEnumerable<MovieCastCrewResponse>>.Failure(Error.NotFound($"Movie with ID {movieId} not found."));
                }
                
                
                movieWithCastCrew.AddRangeCastCrew(castCrewList);

                await _movieRepository.SaveChangesAsync();
                //await _unitOfWork.SaveChangesAsync();


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
                List<MovieCertification> certificationsList = requests.Select(request => new MovieCertification(movieCertifications.Id, request.CertificationBody, request.Rating, request.IssueDate)).ToList();
                movieCertifications.AddRangeCertifications(certificationsList);
                await _movieRepository.UpdateAsync(movieCertifications);
                var response = certificationsList.Select(mc => new MovieCertificationResponse
                {
                    Id = mc.Id,
                    CertificationBody = mc.CertificationBody,
                    Rating = mc.Rating,
                    IssueDate = mc.IssueDate,
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
                List<MovieCopyright> copyrightsList = requests.Select(request => new MovieCopyright(movieWithCopyrights.Id, request.DistributorCompany, request.LicenseStartDate, request.LicenseEndDate, request.Status)).ToList();
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
                List<MovieGenre> genresList = requests.Select(request => new MovieGenre(request.GenreId, movieWithGenres.Id)).ToList();
                movieWithGenres.AddRangeGenres(genresList);

                var genreIds = genresList.Select(g => g.GenreId).ToList();
                var genreSpec = new GenresWithMovieSpecification(genreIds);
                var genres = await _genreRepository.ListAsync(genreSpec);
                if (genres.Count != genreIds.Count)
                {
                    var existingGenreIds = genres.Select(g => g.Id).ToHashSet();
                    var missingGenreIds = genreIds.Where(id => !existingGenreIds.Contains(id)).ToList();
                    return BaseResponse<IEnumerable<MovieGenreResponse>>.Failure(Error.NotFound($"Genres with IDs {string.Join(", ", missingGenreIds)} not found."));
                }
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
                var movie = new Movie(request.Title, request.DurationMinutes, request.ReleaseDate, request.Status, request.Description, request.RatingStatus, request.PosterUrl, request.Trailer);
                await _movieRepository.AddAsync(movie);
                var response = new MovieDetailsResponse
                {
                    Movie = new MovieDetail
                    {
                        Title = movie.Title,
                        DurationMinutes = movie.DurationMinutes,
                        ReleaseDate = movie.ReleaseDate,
                        PosterUrl = movie.PosterUrl,
                        Description = movie.Description,
                    }
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
                var result = movieWithCastCrew.UpdateCastCrew(castCrewId, movieWithCastCrew.Id, request.PersonName, request.RoleType);
                if (!result)
                {
                    return BaseResponse<MovieCastCrewResponse>.Failure(Error.NotFound($"CastCrew with ID {castCrewId} not found"));
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
                    IssueDate = request.IssueDate,
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
                movie.UpdateDetail(request.Title, request.DurationMinutes, request.ReleaseDate, request.Description, request.PosterUrl, request.RatingStatus, request.Trailer);
                await _movieRepository.UpdateAsync(movie);

                var response = new MovieDetailsResponse
                {
                    Movie = new MovieDetail
                    {
                        Title = movie.Title,
                        DurationMinutes = movie.DurationMinutes,
                        ReleaseDate = movie.ReleaseDate,
                        Description = movie.Description,
                        PosterUrl = movie.PosterUrl,
                        CreatedAt = movie.CreatedAt,
                    }
                };
                return BaseResponse<MovieDetailsResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<MovieDetailsResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<MovieComingSoonResponse>>> GetMovieComingSoonAsync()
        {
            try
            {
                var movieWithMGenreSpec = new MovieWithMGenreSpecification();
                var movies = await _movieRepository.ListAsync(movieWithMGenreSpec);

                if (movies == null)
                {
                    return BaseResponse<IEnumerable<MovieComingSoonResponse>>.Success([]);
                }

                var mGenreIds = movies.SelectMany(m => m.MovieGenres).Select(m => m.GenreId).ToHashSet();

                var genreSpec = new GenresWithMovieSpecification(mGenreIds.ToList());
                var genres = await _genreRepository.ListAsync(genreSpec);

                var response = movies.Select(m => new MovieComingSoonResponse
                {
                    MovieId = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Duration = m.DurationMinutes,
                    PostUrl = m.PosterUrl,
                    Trailer = m.Trailer,
                    Genres = m.MovieGenres.Select(mg => genres.FirstOrDefault(g => g.Id == mg.GenreId)?.GenreName ?? "***").ToList()
                });
                return BaseResponse<IEnumerable<MovieComingSoonResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<MovieComingSoonResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<HighlightStat>> GetHighlightStatAsync()
        {
            try
            {
                var baseCinemeSpec = new CinemaBaseSpecification();
                var baseMovieSpec = new MovieBaseSpecification();

                var cinemas = await _cinemaRepository.ListAsync(baseCinemeSpec);
                var movies = await _movieRepository.ListAsync(baseMovieSpec);

                var users = await _userManager.Users.CountAsync();

                var response = new HighlightStat
                {
                    CinemaBaseResponse = cinemas,
                    MovieBaseResponse = movies,
                    StatisticItemResponse = new StatisticItemRessponse
                    {
                        TotalCinemas = cinemas.Count,
                        TotalMovies = movies.Count,
                        TotalUsers = users
                    }
                };
                return BaseResponse<HighlightStat>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<HighlightStat>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<MovieResponse>>> GetMovieListAsync()
        {
            try
            {
                var stMovieSpec = new ShowtimeWithMovieIdSpecification();
                var movieIds = await _showtimeRepository.ListAsync(stMovieSpec);
                if (movieIds.Count == 0)
                {
                    return BaseResponse<IEnumerable<MovieResponse>>.Success([]);
                }

                HashSet<Guid> ids = movieIds.ToHashSet();

                var movieSpec = new MovieWithMGenreSpecification(ids.ToList());
                var movies = await _movieRepository.ListAsync(movieSpec);
                var genreIds = movies.SelectMany(m => m.MovieGenres).Select(mg => mg.GenreId).ToHashSet();

                var genreSpec = new GenresWithMovieSpecification(genreIds.ToList());
                var genres = await _genreRepository.ListAsync(genreSpec);

                var response = movies.Select(m => new MovieResponse
                {
                    Id = m.Id,
                    AgeRating = m.Rating,
                    Description = m.Description,
                    DurationMinutes = m.DurationMinutes,
                    PosterUrl = m.PosterUrl,
                    ReleaseDate = m.ReleaseDate,
                    Title = m.Title,
                    Trailer = m.Trailer,
                    Genres = m.MovieGenres.Select(mg => genres.FirstOrDefault(x => x.Id == mg.GenreId)?.GenreName ?? "***").ToList(),
                });
                return BaseResponse<IEnumerable<MovieResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<MovieResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<MovieSectionResponse>>> GetMoviesSectionAsync()
        {
            try
            {
                var movieSpec = new MovieSectionSpecification();
                var movies = await _movieRepository.ListAsync(movieSpec);
                return BaseResponse<IEnumerable<MovieSectionResponse>>.Success(movies);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<MovieSectionResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
