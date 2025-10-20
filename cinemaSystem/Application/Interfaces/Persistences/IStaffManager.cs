using Domain.Entities.StaffAggregate;
using Shared.Common.Base;
using Shared.Models.DataModels.InventoryDtos;
using Shared.Models.DataModels.StaffDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface IStaffManager
    {
        Task<BaseResponse<IEnumerable<ShiftInfoResponse>>> GetShiftsInfoAsync(Guid cinemaId);
        Task<BaseResponse<IEnumerable<StaffInfoResponse>>> GetStaffInfoAsync(Guid cinemaId, DateTime ShiftDate);
        Task<BaseResponse<IEnumerable<GetStaffToCinemaResponse>>> GetStaffToCinemaAsync(Guid cinemaId);
        Task<BaseResponse<IEnumerable<GetShiftsToCinemaResponse>>> GetShiftsMnAsync(Guid cinemaId);

        Task<BaseResponse<StaffReponse>> GetStaffOnTimeAsync(string email);

        Task<BaseResponse<string>> AddStaffToCinemaAsync(EmployeeCreateRequest request);


        //---------------------Shifts-----------------------------
        Task<BaseResponse<string>> AddShiftsToCinemaAsync(IEnumerable<ShiftRequest> requests);
        Task<BaseResponse<object>> AddShiftToEmployeeAsync(TakeAttendanceOEmpRequest request);

    }
}
