using Shared.Common.Base;
using Shared.Models.DataModels.InventoryDtos;
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
        Task<BaseResponse<IEnumerable<InventoryResponse>>> AddInventoryAsync(IEnumerable<InventoryRequest> requests);
    }
}
