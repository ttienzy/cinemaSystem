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
    /// Dashboard quản lý rạp — Dành cho Manager.
    /// Manager chỉ xem được dữ liệu rạp mình quản lý (cinemaId tự lấy từ StaffAssignment).
    /// </summary>
    [ApiController]
    [Route("api/manager/dashboard")]
    [Authorize(Roles = "Manager")]
    public class ManagerDashboardController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Thống kê tổng quan rạp đang quản lý (doanh thu tháng, bookings, tỉ lệ lấp đầy).
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardSummaryDto>> GetStats([FromQuery] Guid cinemaId)
        {
            return Ok(await mediator.Send(new GetDashboardSummaryQuery(cinemaId)));
        }

        /// <summary>
        /// Báo cáo doanh thu rạp — nhóm theo ngày/tuần/tháng.
        /// Manager dùng để theo dõi tình hình kinh doanh rạp mình.
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
        /// Top phim ăn khách tại rạp — giúp Manager điều chỉnh lịch chiếu phù hợp.
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
