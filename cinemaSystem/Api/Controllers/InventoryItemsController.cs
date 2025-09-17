using Application.Interfaces.Persistences;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Models.DataModels.InventoryDtos;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryItemsController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        public InventoryItemsController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetItemsAsync([FromQuery] Guid CinameId)
        {
            var result = await _inventoryService.GetConcessionsAsync(CinameId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<InventoryResponse>>.WithError(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddItemsAsync([FromBody] IEnumerable<InventoryRequest> requests)
        {
            var result = await _inventoryService.AddInventoryAsync(requests);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<InventoryResponse>>.WithError(result);
        }
    }
}
