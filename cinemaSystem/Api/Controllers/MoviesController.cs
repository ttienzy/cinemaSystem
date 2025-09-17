using Application.Interfaces.Persistences;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.CinemaDtos;
using Shared.Models.DataModels.MovieDtos;
using Shared.Models.DataModels.StatisticDto;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IValidator<MovieRequest> _validator;
        public MoviesController(IMovieService movieService, IValidator<MovieRequest> validator)
        {
            _movieService = movieService;
            _validator = validator;
        }
        [HttpGet("section")]
        public async Task<IActionResult> GetMoviesSectionAsync()
        {
            var result = await _movieService.GetMoviesSectionAsync();
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<MovieSectionResponse>>.WithError(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMovies([FromQuery] MovieQueryParameters parameters)
        {
            var serviceResponse = await _movieService.GetMoviesAsync(parameters);
            if (serviceResponse.IsSuccess)
            {
                var paginatedList = serviceResponse.Value;
                var result = new
                {
                    Data = paginatedList,
                    Pagination = new PaginationResponse
                    {
                        Pageindex = paginatedList.PageIndex,
                        TotalPages = paginatedList.TotalPages,
                        Count = paginatedList.Count,
                        HasNextPage = paginatedList.HasNextPage,
                        HasPreviousPage = paginatedList.HasPreviousPage,
                    }
                };
                return Ok(result);
            }

            return ErrorResponse<PaginatedList<MovieResponse>>.WithError(serviceResponse);
        }
        [HttpGet("coming-soon")]
        public async Task<IActionResult> GetMovieComingSoonAsync()
        {
            var serviceResponse = await _movieService.GetMovieComingSoonAsync();
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<IEnumerable<MovieComingSoonResponse>>.WithError(serviceResponse);
        }
        [HttpGet("feature")]
        public async Task<IActionResult> GetMovieListAsync()
        {
            var serviceResponse = await _movieService.GetMovieListAsync();
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<IEnumerable<MovieResponse>>.WithError(serviceResponse);
        }
        [HttpGet("movie-and-cinema-info")]
        public async Task<IActionResult> GetHighlightStatAsync()
        {
            var serviceResponse = await _movieService.GetHighlightStatAsync();
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<HighlightStat>.WithError(serviceResponse);
        }

        [HttpGet("{movieId:guid}")]
        public async Task<IActionResult> GetMovieById(Guid movieId)
        {
            var serviceResponse = await _movieService.GetMovieByIdAsync(movieId);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<MovieDetailsResponse>.WithError(serviceResponse);
        }
        [HttpPost]
        public async Task<IActionResult> CreateMovie([FromBody] MovieRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }
            var serviceResponse = await _movieService.CreateMovieAsync(request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<MovieDetailsResponse>.WithError(serviceResponse);
        }
        [HttpPost("{movieId:guid}/castcrew")]
        public async Task<IActionResult> AddCastCrewToMovie(Guid movieId, [FromBody] IEnumerable<MovieCastCrewRequest> requests)
        {
            var serviceResponse = await _movieService.AddCastCrewToMovie(movieId, requests);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<IEnumerable<MovieCastCrewResponse>>.WithError(serviceResponse);
        }
        [HttpPost("{movieId:guid}/certifications")]
        public async Task<IActionResult> AddCertificationsToMovie(Guid movieId, [FromBody] IEnumerable<MovieCertificationRequest> requests)
        {
            var serviceResponse = await _movieService.AddCertificationsToMovie(movieId, requests);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<IEnumerable<MovieCertificationResponse>>.WithError(serviceResponse);
        }
        [HttpPost("{movieId:guid}/copyrights")]
        public async Task<IActionResult> AddCopyrightsToMovie(Guid movieId, [FromBody] IEnumerable<MovieCopyrightRequest> requests)
        {
            var serviceResponse = await _movieService.AddCopyrightsToMovie(movieId, requests);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<IEnumerable<MovieCopyrightResponse>>.WithError(serviceResponse);
        }
        [HttpPost("{movieId:guid}/genres")]
        public async Task<IActionResult> AddGenreToMovie(Guid movieId, [FromBody] IEnumerable<MovieGenreRequest> requests)
        {
            var serviceResponse = await _movieService.AddGenreToMovie(movieId, requests);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<IEnumerable<MovieGenreResponse>>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}")]
        public async Task<IActionResult> UpdateMovie(Guid movieId, [FromBody] MovieRequest request)
        {
            var serviceResponse = await _movieService.UpdateMovieAsync(movieId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<MovieDetailsResponse>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}/castcrew/{castCrewId:guid}")]
        public async Task<IActionResult> UpdateCastCrewForMovie(Guid movieId, Guid castCrewId, [FromBody] MovieCastCrewRequest request)
        {
            var serviceResponse = await _movieService.UpdateCastCrewForMovie(movieId, castCrewId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<MovieCastCrewResponse>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}/certifications/{certId:guid}")]
        public async Task<IActionResult> UpdateCertificationsForMovie(Guid movieId, Guid certId, [FromBody] MovieCertificationRequest request)
        {
            var serviceResponse = await _movieService.UpdateCertificationsForMovie(movieId, certId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<MovieCertificationResponse>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}/copyrights/{copyrightId:guid}")]
        public async Task<IActionResult> UpdateCopyrightsForMovie(Guid movieId, Guid copyrightId, [FromBody] MovieCopyrightRequest request)
        {
            var serviceResponse = await _movieService.UpdateCopyrightsForMovie(movieId, copyrightId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<MovieCopyrightResponse>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}/genres/{mGenreId:guid}")]
        public async Task<IActionResult> UpdateGenreForMovie(Guid movieId, Guid mGenreId, [FromBody] MovieGenreRequest request)
        {
            var serviceResponse = await _movieService.UpdateGenreForMovie(movieId, mGenreId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorResponse<MovieGenreResponse>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}")]
        public async Task<IActionResult> DeleteMovie(Guid movieId)
        {
            var serviceResponse = await _movieService.DeleteMovieAsync(movieId);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorResponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}/castcrew")]
        public async Task<IActionResult> DeleteCastCrewForMovie(Guid movieId, [FromBody] IEnumerable<Guid> castCrewIds)
        {
            var serviceResponse = await _movieService.DeleteCastCrewForMovie(movieId, castCrewIds);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorResponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}/certifications")]
        public async Task<IActionResult> DeleteCertificationsForMovie(Guid movieId, [FromBody] IEnumerable<Guid> certificationIds)
        {
            var serviceResponse = await _movieService.DeleteCertificationsForMovie(movieId, certificationIds);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorResponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}/copyrights")]
        public async Task<IActionResult> DeleteCopyrightsForMovie(Guid movieId, [FromBody] IEnumerable<Guid> copyrightIds)
        {
            var serviceResponse = await _movieService.DeleteCopyrightsForMovie(movieId, copyrightIds);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorResponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}/genres")]
        public async Task<IActionResult> DeleteGenreForMovie(Guid movieId, [FromBody] IEnumerable<Guid> genreIds)
        {
            var serviceResponse = await _movieService.DeleteGenreForMovie(movieId, genreIds);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorResponse<object>.WithError(serviceResponse);
        }
    }
}
