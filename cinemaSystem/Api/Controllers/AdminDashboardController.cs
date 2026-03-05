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
    /// Dashboard quản trị toàn chuỗi — Chỉ dành cho Admin.
    /// Cung cấp báo cáo doanh thu, thống kê tổng quan, top phim ăn khách.
    /// </summary>
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Lấy thống kê tổng quan toàn chuỗi rạp (doanh thu tháng, bookings, tỉ lệ lấp đầy).
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardSummaryDto>> GetStats([FromQuery] Guid? cinemaId = null)
        {
            return Ok(await mediator.Send(new GetDashboardSummaryQuery(cinemaId)));
        }

        /// <summary>
        /// Báo cáo doanh thu theo khoảng thời gian — hỗ trợ nhóm theo ngày/tuần/tháng.
        /// Dùng cho biểu đồ doanh thu trên dashboard Admin.
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
        /// Top phim ăn khách nhất — sắp xếp theo tổng doanh thu.
        /// Dùng cho bảng xếp hạng trên dashboard Admin.
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
