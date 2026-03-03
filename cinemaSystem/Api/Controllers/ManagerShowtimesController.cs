using Application.Features.Showtimes.Commands.CreateShowtime;
using Application.Features.Showtimes.Commands.UpdateShowtime;
using Application.Features.Showtimes.Commands.DeleteShowtime;
using Application.Features.Showtimes.Queries.GetShowtimesByCinema;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    /// <summary>
    /// Manager-facing Showtime management.
    /// Managers can only operate on their assigned cinema (enforced at handler level via Staff-Cinema mapping).
    /// Admins can operate on any cinema.
    /// </summary>
    // [Authorize(Roles = "Manager,Admin")]
    [ApiController]
    [Route("api/manager/showtimes")]
    public class ManagerShowtimesController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Schedule a new showtime. Conflict detection + cleaning offset (20 min) enforced.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CreateShowtimeResult), 201)]
        public async Task<IActionResult> CreateShowtime([FromBody] ShowtimeUpsertRequest request)
        {
            var result = await mediator.Send(new CreateShowtimeCommand(request));
            return CreatedAtAction(nameof(GetByCinema),
                new { cinemaId = request.CinemaId, date = request.ShowDate.ToString("yyyy-MM-dd") },
                result);
        }

        /// <summary>
        /// Reschedule an existing showtime. Blocked if any tickets have been sold.
        /// </summary>
        [HttpPut("{showtimeId:guid}")]
        [ProducesResponseType(typeof(UpdateShowtimeResult), 200)]
        public async Task<IActionResult> UpdateShowtime(Guid showtimeId, [FromBody] ShowtimeUpsertRequest request)
        {
            var result = await mediator.Send(new UpdateShowtimeCommand(showtimeId, request));
            return Ok(result);
        }

        /// <summary>
        /// Cancel a showtime (soft delete). Blocked if any tickets have been sold.
        /// </summary>
        [HttpDelete("{showtimeId:guid}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> CancelShowtime(Guid showtimeId)
        {
            await mediator.Send(new DeleteShowtimeCommand(showtimeId));
            return NoContent();
        }

        /// <summary>
        /// Get all showtimes for a cinema on a given date. Manager can only see their own cinema.
        /// </summary>
        [HttpGet("{cinemaId:guid}")]
        [ProducesResponseType(typeof(List<ShowtimeDetailResponse>), 200)]
        public async Task<IActionResult> GetByCinema(Guid cinemaId, [FromQuery] DateTime? date)
        {
            var result = await mediator.Send(new GetShowtimesByCinemaQuery(cinemaId, date ?? DateTime.Today));
            return Ok(result);
        }
    }
}
