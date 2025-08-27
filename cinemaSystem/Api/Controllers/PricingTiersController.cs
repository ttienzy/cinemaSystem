using Application.Interfaces.Persistences;
using Domain.Entities.SharedAggregates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricingTiersController : ControllerBase
    {
        private readonly IPricingTierService _pricingTierService;
        public PricingTiersController(IPricingTierService pricingTierService)
        {
            _pricingTierService = pricingTierService ?? throw new ArgumentNullException(nameof(pricingTierService));
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _pricingTierService.GetPricingTiersAsync();
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorReponse<IEnumerable<PricingTier>>.WithError(response);
        }
        [HttpGet("{pricingTierId:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid pricingTierId)
        {
            var response = await _pricingTierService.GetPricingTierByIdAsync(pricingTierId);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorReponse<PricingTier>.WithError(response);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] PricingTierRequest request)
        {
            if (request == null)
            {
                return BadRequest("Pricing tier request cannot be null.");
            }
            var response = await _pricingTierService.CreatePricingTierAsync(request);
            if (response.IsSuccess)
            {
                return CreatedAtAction(nameof(GetByIdAsync), new { pricingTierId = response.Value.Id }, response.Value);
            }
            return ErrorReponse<PricingTier>.WithError(response);
        }
        [HttpPut("{pricingTierId:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid pricingTierId, [FromBody] PricingTierRequest request)
        {
            if (request == null)
            {
                return BadRequest("Pricing tier request cannot be null.");
            }
            var response = await _pricingTierService.UpdatePricingTierAsync(pricingTierId, request);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorReponse<PricingTier>.WithError(response);
        }
        [HttpDelete("{pricingTierId:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid pricingTierId)
        {
            var response = await _pricingTierService.DeletePricingTierAsync(pricingTierId);
            if (response.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(response);
        }
    }
}
