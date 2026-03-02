using Application.Features.Shared.PricingTiers.Queries.GetAll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.SharedDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class PricingTiersController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<PricingTierDto>>> GetAll()
        {
            return Ok(await Mediator.Send(new GetAllPricingTiersQuery()));
        }

        // Additional mutation endpoints should use [Authorize(Roles = "Admin")]
    }
}
