using Application.Features.Shared.TimeSlots.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Quản lý Khung giờ (TimeSlot) — Chỉ dành cho Admin.
    /// Admin tạo/sửa/xóa khung giờ chiếu phim (Sáng, Chiều, Tối, Khuya).
    /// </summary>
    [ApiController]
    [Route("api/admin/time-slots")]
    [Authorize(Roles = "Admin")]
    public class AdminTimeSlotsController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Tạo khung giờ mới — ví dụ: "Khung giờ vàng 18:00–21:00".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] TimeSlotRequest request)
        {
            var id = await mediator.Send(new CreateTimeSlotCommand(request.StartTime, request.EndTime, request.dateType));
            return Ok(new { id });
        }

        /// <summary>
        /// Cập nhật khung giờ — thay đổi tên, giờ bắt đầu/kết thúc.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TimeSlotRequest request)
        {
            await mediator.Send(new UpdateTimeSlotCommand(id, request.Name, request.StartTime, request.EndTime, request.dateType, request.isActive));
            return NoContent();
        }

        /// <summary>
        /// Xóa khung giờ.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeleteTimeSlotCommand(id));
            return NoContent();
        }
    }

    /// <summary>Request body cho TimeSlot CUD.</summary>
    public record TimeSlotRequest(string Name, TimeSpan StartTime, TimeSpan EndTime, string dateType, bool isActive = true);
}
