using Domain.Entities.InventoryAggregate;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.InventoryDtos;
using Shared.Models.DataModels.StaffDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface IInventoryManager
    {
        Task<BaseResponse<IEnumerable<InventoryResponse>>> GetConcessionsAsync(Guid cinemaId);
        Task<BaseResponse<IEnumerable<InventoryItem>>> GetConcessionItemsDetailAsync(Guid cinemaId);
        Task<BaseResponse<PaginatedList<ConcessionSaleHistoryResponse>>> GetConcessionSaleHistoryAsync(Guid cinemaId, ConcessionSaleQueryParameter query);
        Task<BaseResponse<IEnumerable<ConcessionRevenueResponse>>> GetConcessionRevenueReportAsync(Guid cinemaId);
        Task<BaseResponse<IEnumerable<InventoryResponse>>> AddInventoryAsync(IEnumerable<InventoryRequest> requests);
        Task<BaseResponse<string>> ConfirmPaymentConcessionAsync(CartRequest request, Guid cinemaId);
        Task<BaseResponse<IEnumerable<RevenueReportResponseDto>>> GetRevenueReportAsync(RevenueReportRequestDto request);
        Task<BaseResponse<IEnumerable<RevenueMonthlyReportResponseDto>>> GetMonthlyRevenueReportAsync(RevenueMonthlyReportRequestDto request);
    }
}
