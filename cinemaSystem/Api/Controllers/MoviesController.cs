using Application.Interfaces.Persistences;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.MovieDtos;

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

            return ErrorReponse<PaginatedList<MovieResponse>>.WithError(serviceResponse);
        }
        [HttpGet("{movieId:guid}")]
        public async Task<IActionResult> GetMovieById(Guid movieId)
        {
            var serviceResponse = await _movieService.GetMovieByIdAsync(movieId);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<MovieDetailsResponse>.WithError(serviceResponse);
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
                return CreatedAtAction(nameof(GetMovieById), new { movieId = serviceResponse.Value.Id }, serviceResponse.Value);
            }
            return ErrorReponse<MovieDetailsResponse>.WithError(serviceResponse);
        }
        [HttpPost("{movieId:guid}/castcrew")]
        public async Task<IActionResult> AddCastCrewToMovie(Guid movieId, [FromBody] IEnumerable<MovieCastCrewRequest> requests)
        {
            var serviceResponse = await _movieService.AddCastCrewToMovie(movieId, requests);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<IEnumerable<MovieCastCrewResponse>>.WithError(serviceResponse);
        }
        [HttpPost("{movieId:guid}/certifications")]
        public async Task<IActionResult> AddCertificationsToMovie(Guid movieId, [FromBody] IEnumerable<MovieCertificationRequest> requests)
        {
            var serviceResponse = await _movieService.AddCertificationsToMovie(movieId, requests);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<IEnumerable<MovieCertificationResponse>>.WithError(serviceResponse);
        }
        [HttpPost("{movieId:guid}/copyrights")]
        public async Task<IActionResult> AddCopyrightsToMovie(Guid movieId, [FromBody] IEnumerable<MovieCopyrightRequest> requests)
        {
            var serviceResponse = await _movieService.AddCopyrightsToMovie(movieId, requests);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<IEnumerable<MovieCopyrightResponse>>.WithError(serviceResponse);
        }
        [HttpPost("{movieId:guid}/genres")]
        public async Task<IActionResult> AddGenreToMovie(Guid movieId, [FromBody] IEnumerable<MovieGenreRequest> requests)
        {
            var serviceResponse = await _movieService.AddGenreToMovie(movieId, requests);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<IEnumerable<MovieGenreResponse>>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}")]
        public async Task<IActionResult> UpdateMovie(Guid movieId, [FromBody] MovieRequest request)
        {
            var serviceResponse = await _movieService.UpdateMovieAsync(movieId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<MovieDetailsResponse>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}/castcrew/{castCrewId:guid}")]
        public async Task<IActionResult> UpdateCastCrewForMovie(Guid movieId, Guid castCrewId, [FromBody] MovieCastCrewRequest request)
        {
            var serviceResponse = await _movieService.UpdateCastCrewForMovie(movieId, castCrewId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<MovieCastCrewResponse>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}/certifications/{certId:guid}")]
        public async Task<IActionResult> UpdateCertificationsForMovie(Guid movieId, Guid certId, [FromBody] MovieCertificationRequest request)
        {
            var serviceResponse = await _movieService.UpdateCertificationsForMovie(movieId, certId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<MovieCertificationResponse>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}/copyrights/{copyrightId:guid}")]
        public async Task<IActionResult> UpdateCopyrightsForMovie(Guid movieId, Guid copyrightId, [FromBody] MovieCopyrightRequest request)
        {
            var serviceResponse = await _movieService.UpdateCopyrightsForMovie(movieId, copyrightId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<MovieCopyrightResponse>.WithError(serviceResponse);
        }
        [HttpPut("{movieId:guid}/genres/{mGenreId:guid}")]
        public async Task<IActionResult> UpdateGenreForMovie(Guid movieId, Guid mGenreId, [FromBody] MovieGenreRequest request)
        {
            var serviceResponse = await _movieService.UpdateGenreForMovie(movieId, mGenreId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<MovieGenreResponse>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}")]
        public async Task<IActionResult> DeleteMovie(Guid movieId)
        {
            var serviceResponse = await _movieService.DeleteMovieAsync(movieId);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}/castcrew")]
        public async Task<IActionResult> DeleteCastCrewForMovie(Guid movieId, [FromBody] IEnumerable<Guid> castCrewIds)
        {
            var serviceResponse = await _movieService.DeleteCastCrewForMovie(movieId, castCrewIds);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}/certifications")]
        public async Task<IActionResult> DeleteCertificationsForMovie(Guid movieId, [FromBody] IEnumerable<Guid> certificationIds)
        {
            var serviceResponse = await _movieService.DeleteCertificationsForMovie(movieId, certificationIds);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}/copyrights")]
        public async Task<IActionResult> DeleteCopyrightsForMovie(Guid movieId, [FromBody] IEnumerable<Guid> copyrightIds)
        {
            var serviceResponse = await _movieService.DeleteCopyrightsForMovie(movieId, copyrightIds);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{movieId:guid}/genres")]
        public async Task<IActionResult> DeleteGenreForMovie(Guid movieId, [FromBody] IEnumerable<Guid> genreIds)
        {
            var serviceResponse = await _movieService.DeleteGenreForMovie(movieId, genreIds);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(serviceResponse);
        }
    }
}
