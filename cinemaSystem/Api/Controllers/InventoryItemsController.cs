using Application.Interfaces.Persistences;
using Domain.Entities.InventoryAggregate;
using Infrastructure.Identity.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.InventoryDtos;
using Shared.Models.DataModels.StaffDtos;

namespace Api.Controllers
{
    //[Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Manager},{RoleConstant.Employee}")]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryItemsController : ControllerBase
    {
        private readonly IInventoryManager _inventoryService;
        public InventoryItemsController(IInventoryManager inventoryService)
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
        [HttpGet("details")]
        public async Task<IActionResult> GetItemDetailsAsync([FromQuery] Guid itemId)
        {
            var result = await _inventoryService.GetConcessionItemsDetailAsync(itemId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<InventoryItem>>.WithError(result);
        }

        [HttpGet("sale-history/{cinemaId:guid}")]
        public async Task<IActionResult> GetConcessionSaleHistoryAsync(Guid cinemaId, [FromQuery] ConcessionSaleQueryParameter query)
        {
            var result = await _inventoryService.GetConcessionSaleHistoryAsync(cinemaId, query);
            if (result.IsSuccess)
            {
                var pagingList = result.Value;
                var response = new
                {
                    Data = pagingList,
                    Pagination = new PaginationResponse
                    {
                        Count = pagingList.Count,
                        TotalPages = pagingList.TotalPages,
                        HasNextPage = pagingList.HasNextPage,
                        HasPreviousPage = pagingList.HasPreviousPage,
                        PageIndex = pagingList.PageIndex
                    }
                };
                return Ok(response);
            }
            return ErrorResponse<PaginatedList<ConcessionSaleHistoryResponse>>.WithError(result);
        }

        [HttpGet("revenue-report/{cinemaId:guid}")]
        public async Task<IActionResult> GetConcessionRevenueReportAsync(Guid cinemaId)
        {
            var result = await _inventoryService.GetConcessionRevenueReportAsync(cinemaId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<ConcessionRevenueResponse>>.WithError(result);
        }
        [HttpGet("revenue-report-daily/{cinemaId:guid}")]
        public async Task<IActionResult> GetRevenueReportAsync(Guid cinemaId, [FromQuery] RevenueReportRequestDto request)
        {
            request.CinemaId = cinemaId;
            var result = await _inventoryService.GetRevenueReportAsync(request);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<RevenueReportResponseDto>>.WithError(result);
        }

        [HttpGet("revenue-report-monthly/{cinemaId:guid}")]
        public async Task<IActionResult> GetMonthlyRevenueReportAsync(Guid cinemaId, [FromQuery] RevenueMonthlyReportRequestDto request)
        {
            request.CinemaId = cinemaId;
            var result = await _inventoryService.GetMonthlyRevenueReportAsync(request);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<RevenueMonthlyReportResponseDto>>.WithError(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddItemsAsync([FromBody] IEnumerable<InventoryRequest> requests)
        {
            var result = await _inventoryService.AddInventoryAsync(requests);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<InventoryResponse>>.WithError(result);
        }

        [HttpPost("confirm-payment/{cinemaId:guid}")]
        public async Task<IActionResult> ConfirmPaymentConcessionAsync(Guid cinemaId, [FromBody] CartRequest request)
        {
            var result = await _inventoryService.ConfirmPaymentConcessionAsync(request, cinemaId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<string>.WithError(result);
        }
    }
}
