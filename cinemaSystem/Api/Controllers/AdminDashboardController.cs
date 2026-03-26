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
    /// Chain-wide administration dashboard — Admin only.
    /// Provides revenue reports, general statistics, and top-grossing movies.
    /// </summary>
    [ApiController]
    [Route("api/admin/dashboard")]
    // [Authorize(Roles = "Admin")]
    public class AdminDashboardController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Get general statistics for the entire cinema chain (monthly revenue, bookings, occupancy rate).
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardSummaryDto>> GetStats([FromQuery] Guid? cinemaId = null)
        {
            return Ok(await mediator.Send(new GetDashboardSummaryQuery(cinemaId)));
        }

        /// <summary>
        /// Revenue report over a time period — supports grouping by day/week/month.
        /// Used for revenue charts on the Admin dashboard.
        /// </summary>
        [HttpGet("revenue")]
        public async Task<ActionResult<RevenueReportDto>> GetRevenue(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] Guid? cinemaId = null,
            [FromQuery] string groupBy = "day")
        {
            return Ok(await mediator.Send(new GetRevenueReportQuery(from, to, cinemaId, groupBy)));
        }

        /// <summary>
        /// Top grossing movies — sorted by total revenue.
        /// Used for rankings on the Admin dashboard.
        /// </summary>
        [HttpGet("top-movies")]
        public async Task<ActionResult<List<TopMovieDto>>> GetTopMovies(
            [FromQuery] int limit = 10,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            return Ok(await mediator.Send(new GetTopMoviesQuery(limit, from, to)));
        }
    }
}
