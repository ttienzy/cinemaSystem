using Application.Features.Shared.PricingTiers.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Pricing Tier management — Admin only.
    /// Admin creates/updates/deletes ticket pricing tiers based on seat types and time slots.
    /// </summary>
    [ApiController]
    [Route("api/admin/pricing-tiers")]
    // [Authorize(Roles = "Admin")]
    public class AdminPricingTiersController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Create a new pricing tier — e.g., "VIP Seat - Prime Time = 150,000 VND".
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PricingTierRequest request)
        {
            var id = await mediator.Send(new CreatePricingTierCommand(request.Name, request.BasePrice, request.Description));
            return Ok(new { id });
        }

        /// <summary>
        /// Update pricing tier — change price or name.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PricingTierRequest request)
        {
            await mediator.Send(new UpdatePricingTierCommand(id, request.Name, request.BasePrice, request.Description));
            return NoContent();
        }

        /// <summary>
        /// Delete pricing tier.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeletePricingTierCommand(id));
            return NoContent();
        }
    }

    /// <summary>Request body for PricingTier CUD.</summary>
    public record PricingTierRequest(string Name, decimal BasePrice, string? Description = null);
}
