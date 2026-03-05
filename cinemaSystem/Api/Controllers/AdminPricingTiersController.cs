using Application.Features.Shared.PricingTiers.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Quản lý Bảng giá (PricingTier) — Chỉ dành cho Admin.
    /// Admin tạo/sửa/xóa bảng giá vé theo loại ghế và khung giờ.
    /// </summary>
    [ApiController]
    [Route("api/admin/pricing-tiers")]
    [Authorize(Roles = "Admin")]
    public class AdminPricingTiersController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Tạo bảng giá mới — ví dụ: "Ghế VIP - Khung giờ vàng = 150,000 VND".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PricingTierRequest request)
        {
            var id = await mediator.Send(new CreatePricingTierCommand(request.Name, request.BasePrice, request.Description));
            return Ok(new { id });
        }

        /// <summary>
        /// Cập nhật bảng giá — thay đổi giá hoặc tên.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PricingTierRequest request)
        {
            await mediator.Send(new UpdatePricingTierCommand(id, request.Name, request.BasePrice, request.Description));
            return NoContent();
        }

        /// <summary>
        /// Xóa bảng giá.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeletePricingTierCommand(id));
            return NoContent();
        }
    }

    /// <summary>Request body cho PricingTier CUD.</summary>
    public record PricingTierRequest(string Name, decimal BasePrice, string? Description = null);
}
