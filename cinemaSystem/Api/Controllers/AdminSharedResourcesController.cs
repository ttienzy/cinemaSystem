using Application.Features.Cinemas.Commands.CreateScreen;
using Application.Features.Cinemas.Commands.CreateSeatsBulk;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.CinemaDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Quản lý Thể loại (Genre) — Chỉ dành cho Admin.
    /// Admin tạo/sửa/xóa thể loại phim. Endpoint GET public nằm ở GenresController.
    /// </summary>
    [ApiController]
    [Route("api/admin/genres")]
    [Authorize(Roles = "Admin")]
    public class AdminGenresController : BaseApiController
    {
        /// <summary>
        /// Tạo thể loại mới — ví dụ: "Hành động", "Kinh dị", "Lãng mạn".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] Shared.Models.DataModels.SharedDtos.CreateGenreRequest request)
        {
            return Ok(await Mediator.Send(new Application.Features.Shared.Genres.Commands.Create.CreateGenreCommand(request)));
        }

        /// <summary>
        /// Cập nhật thể loại.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Shared.Models.DataModels.SharedDtos.UpdateGenreRequest request)
        {
            await Mediator.Send(new Application.Features.Shared.Genres.Commands.Update.UpdateGenreCommand(id, request));
            return NoContent();
        }

        /// <summary>
        /// Xóa thể loại — xóa mềm (deactivate).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteGenreCommand(id));
            return NoContent();
        }
    }

    public record DeleteGenreCommand(Guid Id) : IRequest;

    /// <summary>
    /// Quản lý Loại ghế (SeatType) — Chỉ dành cho Admin.
    /// Admin tạo/sửa/xóa loại ghế. Endpoint GET public nằm ở SeatTypesController.
    /// </summary>
    [ApiController]
    [Route("api/admin/seat-types")]
    [Authorize(Roles = "Admin")]
    public class AdminSeatTypesController : BaseApiController
    {
        /// <summary>
        /// Tạo loại ghế mới — ví dụ: "VIP", "Standard", "Couple".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] Shared.Models.DataModels.SharedDtos.CreateSeatTypeRequest request)
        {
            return Ok(await Mediator.Send(new Application.Features.Shared.SeatTypes.Commands.Create.CreateSeatTypeCommand(request)));
        }

        /// <summary>
        /// Cập nhật loại ghế.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Shared.Models.DataModels.SharedDtos.UpdateSeatTypeRequest request)
        {
            await Mediator.Send(new Application.Features.Shared.SeatTypes.Commands.Update.UpdateSeatTypeCommand(id, request));
            return NoContent();
        }

        /// <summary>
        /// Xóa loại ghế — xóa mềm (deactivate).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteSeatTypeCommand(id));
            return NoContent();
        }
    }

    public record DeleteSeatTypeCommand(Guid Id) : IRequest;
}
