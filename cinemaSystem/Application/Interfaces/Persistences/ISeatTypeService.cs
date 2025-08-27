using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface ISeatTypeService
    {
        Task<BaseResponse<IEnumerable<SeatType>>> GetSeatTypesAsync();
        Task<BaseResponse<SeatType>> GetSeatTypeByIdAsync(Guid seatTypeId);
        Task<BaseResponse<SeatType>> CreateSeatTypeAsync(SeatTypeRequest request);
        Task<BaseResponse<SeatType>> UpdateSeatTypeAsync(Guid seatTypeId, SeatTypeRequest request);
        Task<BaseResponse<object>> DeleteSeatTypeAsync(Guid seatTypeId);
    }
}
