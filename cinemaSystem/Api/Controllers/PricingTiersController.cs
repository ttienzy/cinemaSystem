using Application.Features.Shared.PricingTiers.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.SharedDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    // [Authorize(Roles = "Admin,Manager")]
    public class PricingTiersController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<List<PricingTierDto>>> GetAll()
        {
            return Ok(await Mediator.Send(new GetAllPricingTiersQuery()));
        }
        
        // Additional endpoints can be added for creation/updates.
    }
}
