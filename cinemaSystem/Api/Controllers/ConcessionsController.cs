using Application.Features.Concessions.Commands.CreateConcessionSale;
using Application.Features.Concessions.Queries.GetConcessionSales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    public class ConcessionsController : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateSale([FromBody] CreateConcessionSaleCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpGet("{cinemaId}")]
        [Authorize(Roles = "Manager,Admin,Staff")]
        public async Task<ActionResult<ConcessionSalesResponse>> GetSales(
            Guid cinemaId, 
            [FromQuery] DateTime? from, 
            [FromQuery] DateTime? to, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            return Ok(await Mediator.Send(new GetConcessionSalesQuery(cinemaId, from, to, page, pageSize)));
        }
    }
}
