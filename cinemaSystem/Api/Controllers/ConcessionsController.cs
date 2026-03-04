using Application.Features.Concessions.Commands.CreateSale;
using Application.Features.Concessions.Queries.GetConcessionSales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Handles concession sales APIs.
    /// </summary>
    public class ConcessionsController : BaseApiController
    {
        /// <summary>
        /// Create a new concession sale.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateSale([FromBody] CreateConcessionSaleCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Get concession sales by cinema.
        /// </summary>
        [HttpGet("{cinemaId}")]
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
