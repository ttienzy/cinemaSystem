using Application.Features.Dashboard.Queries.GetDashboardSummary;
using Application.Features.Dashboard.Queries.GetRevenueReport;
using Application.Features.Dashboard.Queries.GetTopMovies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.DashboardDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Cinema Management Dashboard — For Managers.
    /// Managers can only view data for the cinema they manage.
    /// </summary>
    [ApiController]
    [Route("api/manager/dashboard")]
    // [Authorize(Roles = "Manager")]
    public class ManagerDashboardController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// General statistics of the managed cinema (monthly revenue, bookings, occupancy rate).
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardSummaryDto>> GetStats([FromQuery] Guid cinemaId)
        {
            return Ok(await mediator.Send(new GetDashboardSummaryQuery(cinemaId)));
        }

        /// <summary>
        /// Cinema revenue report — grouped by day/week/month.
        /// Managers use this to track their cinema's business performance.
        /// </summary>
        [HttpGet("revenue")]
        public async Task<ActionResult<RevenueReportDto>> GetRevenue(
            [FromQuery] Guid cinemaId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string groupBy = "day")
        {
            return Ok(await mediator.Send(new GetRevenueReportQuery(from, to, cinemaId, groupBy)));
        }

        /// <summary>
        /// Top grossing movies at the cinema — helps Managers adjust showtimes accordingly.
        /// </summary>
        [HttpGet("top-movies")]
        public async Task<ActionResult<List<TopMovieDto>>> GetTopMovies(
            [FromQuery] Guid cinemaId,
            [FromQuery] int limit = 10,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            return Ok(await mediator.Send(new GetTopMoviesQuery(limit, from, to, cinemaId)));
        }
    }
}
