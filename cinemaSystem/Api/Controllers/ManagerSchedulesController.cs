using Application.Features.Schedules.Commands.CreateSchedule;
using Application.Features.Schedules.Commands.BulkCreateSchedule;
using Application.Features.Schedules.Queries.GetWeeklySchedule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.StaffDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Quản lý lịch làm nhân viên — Dành cho Manager.
    /// Manager xếp nhân viên vào ca theo ngày, hỗ trợ xếp hàng loạt cả tuần.
    /// Xem lịch tuần dạng bảng: nhân viên x ngày x ca.
    /// </summary>
    [ApiController]
    [Route("api/manager/schedules")]
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerSchedulesController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Xem lịch làm theo tuần — truyền bất kỳ ngày nào trong tuần, hệ thống tự tính Thứ Hai.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<WorkScheduleDto>>> GetWeeklySchedule(
            [FromQuery] Guid cinemaId,
            [FromQuery] DateTime weekOf)
        {
            return Ok(await mediator.Send(new GetWeeklyScheduleQuery(cinemaId, weekOf)));
        }

        /// <summary>
        /// Xếp 1 nhân viên vào 1 ca cụ thể — validate không trùng ngày.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateSchedule([FromBody] ScheduleCreateRequest request)
        {
            var id = await mediator.Send(new CreateScheduleCommand(request));
            return Ok(new { id });
        }

        /// <summary>
        /// Xếp lịch hàng loạt — gửi nhiều entry 1 lần (cả tuần cho nhiều nhân viên).
        /// Trả về số lượng thành công/bỏ qua và danh sách lỗi.
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkScheduleRequest request)
        {
            var result = await mediator.Send(new BulkCreateScheduleCommand(request));
            return Ok(result);
        }
    }
}
