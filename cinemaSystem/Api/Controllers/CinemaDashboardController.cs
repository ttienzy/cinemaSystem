using Application.Interfaces.Persistences.Repo;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.DashboardDtos;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CinemaDashboardController(ICinemaInsightRepository repo) : ControllerBase
{
    // Sử dụng primary constructor cho sự ngắn gọn
    private readonly ICinemaInsightRepository _repo = repo;

    /// <summary>
    /// Lấy báo cáo tổng hợp về doanh thu vé và đồ ăn.
    /// </summary>
    [HttpPost("sales-summary")] // Một endpoint duy nhất cho cả hai
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSalesInsights([FromBody] CinemaDashboardRequest request)
    {
        // Sử dụng pattern matching để kiểm tra null
        if (request is null)
        {
            return BadRequest("Yêu cầu không được để trống.");
        }
        if (request.Day.HasValue && (request.Day < 1 || request.Day > 31))
        {
            return BadRequest("Ngày không hợp lệ.");
        }


        // Gọi phương thức đã hợp nhất
        var (ticketReport, concessionReport) = await _repo.FetchSalesInsightsAsync(request);

        // Tạo một đối tượng anonymous hoặc một DTO mới để trả về một JSON duy nhất
        var combinedReport = new
        {
            Tickets = ticketReport,
            Concessions = concessionReport
        };

        return Ok(combinedReport);
    }

    /// <summary>
    /// Quét tỷ lệ lấp đầy của các suất chiếu.
    /// </summary>
    [HttpGet("showtimes/{cinemaId:guid}")]
    public async Task<IActionResult> ScanShowtimeCapacity(Guid cinemaId)
    {
        if (cinemaId == Guid.Empty)
        {
            return BadRequest("Cinema ID không hợp lệ.");
        }

        var result = await _repo.ScanShowtimeCapacityAsync(cinemaId);
        return Ok(result);
    }

    /// <summary>
    /// Thăm dò tín hiệu tồn kho.
    /// </summary>
    [HttpGet("inventory/{cinemaId:guid}")]
    public async Task<IActionResult> ProbeInventorySignals(Guid cinemaId)
    {
        if (cinemaId == Guid.Empty)
        {
            return BadRequest("Cinema ID không hợp lệ.");
        }

        var result = await _repo.ProbeInventorySignalsAsync(cinemaId);
        return Ok(result);
    }

    /// <summary>
    /// Đánh giá tình hình nhân sự trong ngày.
    /// </summary>
    [HttpGet("staff/{cinemaId:guid}")]
    public async Task<IActionResult> EvaluateStaffLoad(Guid cinemaId)
    {
        if (cinemaId == Guid.Empty)
        {
            return BadRequest("Cinema ID không hợp lệ.");
        }

        var result = await _repo.EvaluateDailyManpowerAsync(cinemaId);
        return Ok(result);
    }
}
