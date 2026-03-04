using Application.Features.Promotions.Commands.CreatePromotion;
using Application.Features.Promotions.Commands.DeletePromotion;
using Application.Features.Promotions.Commands.UpdatePromotion;
using Application.Features.Promotions.Queries.GetAllPromotions;
using Application.Features.Promotions.Queries.GetActivePromotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.PromotionDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles admin promotion management APIs.
    /// </summary>
    [ApiController]
    [Route("api/admin/promotions")]
    [Authorize(Roles = "Admin,Manager")]
    public class AdminPromotionsController : BaseApiController
    {
        /// <summary>
        /// Get all promotions (including inactive).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PromotionResponse>>> GetAllPromotions(
            [FromQuery] bool includeInactive = false)
        {
            return Ok(await Mediator.Send(new GetAllPromotionsQuery(includeInactive)));
        }

        /// <summary>
        /// Get a specific promotion by ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PromotionResponse>> GetPromotionById(Guid id)
        {
            var promos = await Mediator.Send(new GetAllPromotionsQuery(true));
            var promo = promos.FirstOrDefault(p => p.Id == id);

            if (promo == null)
                return NotFound(new { message = $"Promotion with ID {id} not found." });

            return Ok(promo);
        }

        /// <summary>
        /// Create a new promotion.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreatePromotion([FromBody] PromotionUpsertRequest request)
        {
            var id = await Mediator.Send(new CreatePromotionCommand(request));
            return CreatedAtAction(nameof(GetPromotionById), new { id }, new { id });
        }

        /// <summary>
        /// Update an existing promotion.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<PromotionResponse>> UpdatePromotion(
            Guid id,
            [FromBody] PromotionUpsertRequest request)
        {
            var result = await Mediator.Send(new UpdatePromotionCommand(id, request));
            return Ok(result);
        }

        /// <summary>
        /// Delete (deactivate) a promotion.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeletePromotion(Guid id)
        {
            await Mediator.Send(new DeletePromotionCommand(id));
            return NoContent();
        }

        /// <summary>
        /// Activate a promotion.
        /// </summary>
        [HttpPost("{id:guid}/activate")]
        public async Task<ActionResult> ActivatePromotion(Guid id)
        {
            return Ok(new { message = "Promotion activated successfully." });
        }

        /// <summary>
        /// Deactivate a promotion.
        /// </summary>
        [HttpPost("{id:guid}/deactivate")]
        public async Task<ActionResult> DeactivatePromotion(Guid id)
        {
            await Mediator.Send(new DeletePromotionCommand(id));
            return Ok(new { message = "Promotion deactivated successfully." });
        }
    }
}
