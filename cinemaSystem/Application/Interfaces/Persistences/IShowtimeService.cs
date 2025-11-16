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
        Task<BaseResponse<IEnumerable<ShowtimeFeaturedResponse>>> GetShowtimesAsync(ShowtimeQueryParameters parameters);
        Task<BaseResponse<ShowtimeResponse>> GetShowtimeByIdAsync(Guid id);
        Task<BaseResponse<ShowtimeSeatingPlanResponse>> GetShowtimeSeatingPlanAsync(Guid showtimeId);
        Task<BaseResponse<ShowtimeFeaturedResponse>> GetShowtimeFeaturedAsync(ShowtimeQueryParameters parameters);
        Task<BaseResponse<IEnumerable<ShowtimePerformanceDto>>> GetShowtimePerformanceAsync(Guid cinemaId);
        Task<BaseResponse<ShowtimeSetupDataDto>> GetShowtimeSetupDataAsync(Guid cinemaId);
        Task<BaseResponse<ShowtimeResponse>> CreateShowtimeAsync(ShowtimeRequest request);
        Task<BaseResponse<ShowtimePricingResponse>> AddPricingToShowtimeAsync(Guid showtimeId, ShowtimePricingRequest request);

        Task<BaseResponse<ShowtimeResponse>> UpdateShowtimeAsync(Guid id, ShowtimeRequest request);
        Task<BaseResponse<ShowtimePricingResponse>> UpdatePricingToShowtimeAsync(Guid showtimeId, Guid pricingId, ShowtimePricingRequest request);

        Task<BaseResponse<object>> ConfirmedShowtimeAsync(Guid showtimeId);
        Task<BaseResponse<object>> CancelledShowtimeAsync(Guid showtimeId);
        Task<BaseResponse<object>> DeletePricingFromShowtimeAsync(Guid showtimeId, Guid pricingId);

    }
}
