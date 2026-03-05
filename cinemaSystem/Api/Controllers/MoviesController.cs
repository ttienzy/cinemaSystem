using Application.Features.Movies.Queries.GetMovieDetails;
using Application.Features.Movies.Queries.GetMoviesPaged;
using Application.Features.Movies.Queries.GetNowShowing;
using Application.Features.Movies.Queries.GetComingSoon;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.MovieDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles movie-related public APIs.
    /// </summary>
    public class MoviesController : BaseApiController
    {
        /// <summary>
        /// Get paginated list of movies.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedMovieResponse>> GetMovies([FromQuery] GetMoviesPagedQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        /// <summary>
        /// Get movie details by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDetailsResponse>> GetMovieDetails(Guid id)
        {
            return Ok(await Mediator.Send(new GetMovieDetailsQuery(id)));
        }

        /// <summary>
        /// Get now showing movies.
        /// </summary>
        [HttpGet("now-showing")]
        public async Task<ActionResult<List<MovieSummaryResponse>>> GetNowShowing([FromQuery] DateTime? date = null)
        {
            return Ok(await Mediator.Send(new GetNowShowingQuery(date)));
        }

        /// <summary>
        /// Get coming soon movies.
        /// </summary>
        [HttpGet("coming-soon")]
        public async Task<ActionResult<List<MovieComingSoonResponse>>> GetComingSoon([FromQuery] DateTime? date = null)
        {
            return Ok(await Mediator.Send(new GetComingSoonQuery(date)));
        }
    }
}
