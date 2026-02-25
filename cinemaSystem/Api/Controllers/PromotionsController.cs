using Application.Features.Promotions.Queries.GetActivePromotions;
using Application.Features.Promotions.Queries.ValidatePromotion;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class PromotionsController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<List<PromotionDto>>> GetActivePromotions()
        {
            return Ok(await Mediator.Send(new GetActivePromotionsQuery()));
        }

        [HttpGet("validate")]
        public async Task<ActionResult<ValidationResult>> ValidatePromotion([FromQuery] string code, [FromQuery] decimal orderTotal)
        {
            return Ok(await Mediator.Send(new ValidatePromotionQuery(code, orderTotal)));
        }
    }
}
