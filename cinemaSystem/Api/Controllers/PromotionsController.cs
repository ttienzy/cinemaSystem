using Application.Features.Promotions.Queries.GetActivePromotions;
using Application.Features.Promotions.Queries.ValidatePromotion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class PromotionsController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<PromotionDto>>> GetActivePromotions()
        {
            return Ok(await Mediator.Send(new GetActivePromotionsQuery()));
        }

        [AllowAnonymous]
        [HttpGet("validate")]
        public async Task<ActionResult<ValidationResult>> ValidatePromotion([FromQuery] string code, [FromQuery] decimal orderTotal)
        {
            return Ok(await Mediator.Send(new ValidatePromotionQuery(code, orderTotal)));
        }

        // Future mutation endpoints (Create/Update/Delete) should use [Authorize(Roles = "Admin,Manager")]
    }
}
