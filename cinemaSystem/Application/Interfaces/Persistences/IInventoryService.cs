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
    public interface IInventoryService
    {
        Task<BaseResponse<IEnumerable<InventoryResponse>>> GetConcessionsAsync(Guid cinemaId);
        Task<BaseResponse<PaginatedList<ConcessionSaleHistoryResponse>>> GetConcessionSaleHistoryAsync(Guid cinemaId, ConcessionSaleQueryParameter query);
        Task<BaseResponse<IEnumerable<StaffInfoResponse>>> GetStaffInfoAsync(Guid cinemaId);
        Task<BaseResponse<IEnumerable<ConcessionRevenueResponse>>> GetConcessionRevenueReportAsync(Guid cinemaId);
        Task<BaseResponse<StaffReponse>> GetStaffOnTimeAsync(string email);
        Task<BaseResponse<IEnumerable<InventoryResponse>>> AddInventoryAsync(IEnumerable<InventoryRequest> requests);
        Task<BaseResponse<string>> ConfirmPaymentConcessionAsync(CartRequest request, Guid cinemaId);
        Task<BaseResponse<string>> AddStaffToCinema(EmployeeCreateRequest request);
    }
}
