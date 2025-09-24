using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.InventoryDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences.Repo
{
    public interface IConcessionSaleRepository
    {
        Task<PaginatedList<ConcessionSaleHistoryResponse>> GetConcessionSaleHistory(Guid cinemaId, ConcessionSaleQueryParameter query);
        Task<IEnumerable<ConcessionRevenueResponse>> GetConcessionRevenueReportAsync(Guid cinemaId);
    }
}
