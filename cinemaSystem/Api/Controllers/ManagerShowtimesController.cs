using Application.Features.Showtimes.Commands.CreateShowtime;
using Application.Features.Showtimes.Commands.UpdateShowtime;
using Application.Features.Showtimes.Commands.DeleteShowtime;
using Application.Features.Showtimes.Commands.BulkCreateShowtimes;
using Application.Features.Showtimes.Commands.ConfirmShowtime;
using Application.Features.Showtimes.Queries.GetShowtimesByCinema;
using Application.Features.Showtimes.Queries.GetShowtimeDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.ShowtimeDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles manager showtime management APIs.
    /// </summary>
    [ApiController]
    [Route("api/manager/showtimes")]
    public class ManagerShowtimesController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Schedule a new showtime.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateShowtime([FromBody] ShowtimeUpsertRequest request)
        {
            var result = await mediator.Send(new CreateShowtimeCommand(request));
            return CreatedAtAction(nameof(GetByCinema),
                new { cinemaId = request.CinemaId, date = request.ShowDate.ToString("yyyy-MM-dd") },
                result);
        }

        /// <summary>
        /// Bulk create multiple showtimes.
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateShowtimes([FromBody] BulkShowtimeRequest request)
        {
            var result = await mediator.Send(new BulkCreateShowtimesCommand(request));
            return Ok(result);
        }

        /// <summary>
        /// Update an existing showtime.
        /// </summary>
        [HttpPut("{showtimeId:guid}")]
        public async Task<IActionResult> UpdateShowtime(Guid showtimeId, [FromBody] ShowtimeUpsertRequest request)
        {
            var result = await mediator.Send(new UpdateShowtimeCommand(showtimeId, request));
            return Ok(result);
        }

        /// <summary>
        /// Cancel a showtime (soft delete).
        /// </summary>
        [HttpDelete("{showtimeId:guid}")]
        public async Task<IActionResult> CancelShowtime(Guid showtimeId)
        {
            await mediator.Send(new DeleteShowtimeCommand(showtimeId));
            return NoContent();
        }

        /// <summary>
        /// Confirm a scheduled showtime.
        /// </summary>
        [HttpPost("{showtimeId:guid}/confirm")]
        public async Task<IActionResult> ConfirmShowtime(Guid showtimeId)
        {
            var result = await mediator.Send(new ConfirmShowtimeCommand(showtimeId));
            return Ok(new { success = result, message = "Showtime confirmed successfully." });
        }

        /// <summary>
        /// Get showtimes by cinema.
        /// </summary>
        [HttpGet("{cinemaId:guid}")]
        public async Task<IActionResult> GetByCinema(Guid cinemaId, [FromQuery] DateTime? date)
        {
            var result = await mediator.Send(new GetShowtimesByCinemaQuery(cinemaId, date ?? DateTime.Today));
            return Ok(result);
        }

        /// <summary>
        /// Get showtime details.
        /// </summary>
        [HttpGet("detail/{showtimeId:guid}")]
        public async Task<IActionResult> GetShowtimeDetails(Guid showtimeId)
        {
            var result = await mediator.Send(new GetShowtimeDetailsQuery(showtimeId));
            return Ok(result);
        }
    }
}
