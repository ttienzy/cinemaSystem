using Application.Features.Movies.Queries.GetMovieDetails;
using Application.Features.Movies.Queries.GetMoviesPaged;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.MovieDtos;

namespace Api.Controllers
{
    public class MoviesController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<PagedMovieResponse>> GetMovies([FromQuery] GetMoviesPagedQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDetailsResponse>> GetMovieDetails(Guid id)
        {
            return Ok(await Mediator.Send(new GetMovieDetailsQuery(id)));
        }
    }
}
