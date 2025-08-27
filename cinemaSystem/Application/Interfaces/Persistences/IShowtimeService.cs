using Shared.Common.Base;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface IShowtimeService
    {
        Task<BaseResponse<IEnumerable<ShowtimeDetailsResponse>>> GetShowtimesAsync(ShowtimeQueryParameters parameters);
        Task<BaseResponse<ShowtimeSeatingPlanResponse>> GetShowtimeSeatingPlanAsync(Guid showtimeId);

        Task<BaseResponse<ShowtimeResponse>> CreateShowtimeAsync(ShowtimeRequest request);
        Task<BaseResponse<ShowtimePricingResponse>> AddPricingToShowtimeAsync(Guid showtimeId, ShowtimePricingRequest request);

        Task<BaseResponse<ShowtimeResponse>> UpdateShowtimeAsync(Guid id, ShowtimeRequest request);
        Task<BaseResponse<ShowtimePricingResponse>> UpdatePricingToShowtimeAsync(Guid showtimeId, Guid pricingId, ShowtimePricingRequest request);

        Task<BaseResponse<object>> DeleteShowtimeAsync(Guid showtimeId);
        Task<BaseResponse<object>> DeletePricingFromShowtimeAsync(Guid showtimeId, Guid pricingId);

    }
}
