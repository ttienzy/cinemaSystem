using Application.Features.Shifts.Commands.CreateShift;
using Application.Features.Shifts.Commands.UpdateShift;
using Application.Features.Shifts.Commands.DeleteShift;
using Application.Features.Shifts.Queries.GetShiftsByCinema;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.StaffDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Quản lý ca làm — Dành cho Manager.
    /// Mỗi rạp có thể có nhiều ca: "Ca Sáng 08:00–14:00", "Ca Chiều 14:00–22:00".
    /// Manager tạo ca, sau đó xếp nhân viên vào ca qua ManagerSchedulesController.
    /// </summary>
    [ApiController]
    [Route("api/manager/shifts")]
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerShiftsController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách ca làm theo rạp — dùng cho dropdown khi xếp lịch.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ShiftDto>>> GetShifts([FromQuery] Guid cinemaId)
        {
            return Ok(await mediator.Send(new GetShiftsByCinemaQuery(cinemaId)));
        }

        /// <summary>
        /// Tạo ca làm mới — ví dụ: "Ca Sáng" từ 08:00 đến 14:00.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateShift([FromBody] ShiftUpsertRequest request)
        {
            var id = await mediator.Send(new CreateShiftCommand(request));
            return CreatedAtAction(nameof(GetShifts), new { cinemaId = request.CinemaId }, new { id });
        }

        /// <summary>
        /// Cập nhật ca làm — thay đổi giờ bắt đầu/kết thúc hoặc tên ca.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateShift(Guid id, [FromBody] ShiftUpsertRequest request)
        {
            await mediator.Send(new UpdateShiftCommand(id, request));
            return NoContent();
        }

        /// <summary>
        /// Xóa ca làm — không xóa được nếu đang có lịch phân công trong tương lai.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteShift(Guid id)
        {
            await mediator.Send(new DeleteShiftCommand(id));
            return NoContent();
        }
    }
}
