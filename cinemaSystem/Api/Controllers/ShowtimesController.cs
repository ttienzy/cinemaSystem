using Application.Features.Showtimes.Queries.GetSeatingPlan;
using Application.Features.Showtimes.Queries.GetShowtimesByMovie;
using Application.Features.Showtimes.Queries.GetShowtimesByCinema;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.ShowtimeDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles public showtime-related APIs.
    /// </summary>
    public class ShowtimesController : BaseApiController
    {
        /// <summary>
        /// Get showtimes by movie ID.
        /// </summary>
        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<List<ShowtimeResponse>>> GetByMovie(Guid movieId, [FromQuery] DateTime? date)
        {
            return Ok(await Mediator.Send(new GetShowtimesByMovieQuery(movieId, date ?? DateTime.Today)));
        }

        /// <summary>
        /// Get showtimes by cinema ID.
        /// </summary>
        [HttpGet("cinema/{cinemaId}")]
        public async Task<ActionResult<List<ShowtimeResponse>>> GetByCinema(Guid cinemaId, [FromQuery] DateTime? date)
        {
            return Ok(await Mediator.Send(new GetShowtimesByCinemaQuery(cinemaId, date ?? DateTime.Today)));
        }

        /// <summary>
        /// Get seating plan for a showtime.
        /// </summary>
        [HttpGet("{id}/seating-plan")]
        public async Task<ActionResult<ShowtimeSeatingPlanResponse>> GetSeatingPlan(Guid id)
        {
            return Ok(await Mediator.Send(new GetSeatingPlanQuery(id)));
        }
    }
}
