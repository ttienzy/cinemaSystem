using Application.Interfaces.Persistences;
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
    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Manager},{RoleConstant.Employee}")]
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
        [HttpGet("schedules/{cinemaId:guid}")]
        public async Task<IActionResult> GetStaffInfoAsync(Guid cinemaId)
        {
            var result = await _inventoryService.GetStaffInfoAsync(cinemaId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<StaffInfoResponse>>.WithError(result);
        }

        [HttpGet("revenue-report/{cinemaId:guid}")]
        public async Task<IActionResult> GetConcessionRevenueReportAsync(Guid cinemaId)
        {
            var result = await _inventoryService.GetConcessionRevenueReportAsync(cinemaId);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<IEnumerable<ConcessionRevenueResponse>>.WithError(result);
        }
        [HttpGet("staff-on-time")]
        public async Task<IActionResult> GetStaffOnTimeAsync([FromQuery] string email)
        {
            var result = await _inventoryService.GetStaffOnTimeAsync(email);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<StaffReponse>.WithError(result);
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
        [HttpPost("add-staff")]
        public async Task<IActionResult> AddStaffToCinemaAsync([FromBody] EmployeeCreateRequest request)
        {
            var result = await _inventoryService.AddStaffToCinema(request);
            if (result.IsSuccess)
                return Ok(result.Value);
            return ErrorResponse<string>.WithError(result);
        }
    }
}
