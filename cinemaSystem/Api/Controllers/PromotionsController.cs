using Application.Features.Promotions.Queries.GetActivePromotions;
using Application.Features.Promotions.Queries.ValidatePromotion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Handles promotion-related APIs.
    /// </summary>
    public class PromotionsController : BaseApiController
    {
        /// <summary>
        /// Get all active promotions.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<PromotionDto>>> GetActivePromotions()
        {
            return Ok(await Mediator.Send(new GetActivePromotionsQuery()));
        }

        /// <summary>
        /// Validate a promotion code.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("validate")]
        public async Task<ActionResult<ValidationResult>> ValidatePromotion([FromQuery] string code, [FromQuery] decimal orderTotal)
        {
            return Ok(await Mediator.Send(new ValidatePromotionQuery(code, orderTotal)));
        }
    }
}
