using Application.Features.Shared.PricingTiers.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.SharedDtos;

namespace Api.Controllers
{
    /// <summary>
    /// Handles pricing tier management APIs.
    /// </summary>
    public class PricingTiersController : BaseApiController
    {
        /// <summary>
        /// Get all pricing tiers.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<PricingTierDto>>> GetAll()
        {
            return Ok(await Mediator.Send(new GetAllPricingTiersQuery()));
        }
    }
}
