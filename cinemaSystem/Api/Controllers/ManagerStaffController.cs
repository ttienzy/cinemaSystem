using Application.Common.Interfaces.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    /// <summary>
    /// Xem nhân viên tại rạp — Dành cho Manager.
    /// Manager xem danh sách staff thuộc rạp mình để xếp ca, phân công.
    /// </summary>
    [ApiController]
    [Route("api/manager/staff")]
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerStaffController(IStaffRepository staffRepo) : ControllerBase
    {
        /// <summary>
        /// Danh sách nhân viên thuộc rạp — lọc theo cinemaId.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStaffByCinema(
            [FromQuery] Guid cinemaId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = staffRepo.GetQueryable()
                .Where(s => s.CinemaId == cinemaId);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.Position,
                    s.Department,
                    s.Phone,
                    s.Email,
                    s.Status,
                    s.HireDate
                })
                .ToListAsync();

            return Ok(new { items, total, page, pageSize });
        }

        /// <summary>
        /// Chi tiết nhân viên — thông tin + lịch làm gần nhất.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetStaffDetail(Guid id)
        {
            var staff = await staffRepo.GetByIdAsync(id);
            if (staff == null) return NotFound(new { message = "Không tìm thấy nhân viên." });

            return Ok(new
            {
                staff.Id,
                staff.FullName,
                staff.Position,
                staff.Department,
                staff.Phone,
                staff.Email,
                staff.Address,
                staff.HireDate,
                staff.Salary,
                staff.Status,
                staff.CinemaId
            });
        }
    }
}
