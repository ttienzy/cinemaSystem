using Application.Features.Inventory.Commands.RestockInventory;
using Application.Features.Inventory.Queries.GetInventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    public class InventoryController : BaseApiController
    {
        [HttpGet("{cinemaId}")]
        public async Task<ActionResult<List<InventoryDto>>> GetInventory(Guid cinemaId, [FromQuery] bool lowStockOnly = false)
        {
            return Ok(await Mediator.Send(new GetInventoryQuery(cinemaId, lowStockOnly)));
        }

        [HttpPost("restock")]
        public async Task<IActionResult> Restock([FromBody] RestockInventoryCommand command)
        {
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
