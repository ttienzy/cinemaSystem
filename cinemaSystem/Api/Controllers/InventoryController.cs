using Application.Features.Inventory.Commands.RestockInventory;
using Application.Features.Inventory.Commands.CreateInventoryItem;
using Application.Features.Inventory.Queries.GetInventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Handles inventory management APIs.
    /// </summary>
    public class InventoryController : BaseApiController
    {
        /// <summary>
        /// Get inventory items by cinema.
        /// </summary>
        [HttpGet("{cinemaId}")]
        public async Task<ActionResult<List<InventoryDto>>> GetInventory(Guid cinemaId, [FromQuery] bool lowStockOnly = false)
        {
            return Ok(await Mediator.Send(new GetInventoryQuery(cinemaId, lowStockOnly)));
        }

        /// <summary>
        /// Create a new inventory item.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateItem([FromBody] CreateInventoryItemRequest request)
        {
            return Ok(await Mediator.Send(new CreateInventoryItemCommand(request)));
        }

        /// <summary>
        /// Restock inventory items.
        /// </summary>
        [HttpPost("restock")]
        public async Task<IActionResult> Restock([FromBody] RestockInventoryCommand command)
        {
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
