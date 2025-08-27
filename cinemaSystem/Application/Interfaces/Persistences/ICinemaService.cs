using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.CinemaDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface ICinemaService
    {
        Task<BaseResponse<PaginatedList<CinemaResponse>>> GetCinemasWithScreensAsync(CinemaQueryParameters parameters);
        Task<BaseResponse<IEnumerable<SeatResponse>>> GetSeatsForScreenAsync(Guid cinemaId, Guid screenId);

        Task<BaseResponse<CinemaResponse>> CreateCinemaAsync(CinemaRequest request);
        Task<BaseResponse<ScreenResponse>> AddScreenToCinemaAsync(Guid cinemaId, ScreenRequest request);
        Task<BaseResponse<IEnumerable<SeatResponse>>> GenerateSeatsForScreenAsync(Guid cinemaId, Guid screenId, IEnumerable<SeatGenerateRequest> requests);

        Task<BaseResponse<CinemaResponse>> UpdateCinemaAsync(Guid cinemaId, CinemaRequest request);
        Task<BaseResponse<ScreenResponse>> UpdateScreenForCinemaAsync(Guid cinemaId, Guid screenId, ScreenRequest request);
        Task<BaseResponse<SeatResponse>> UpdateSeatForScreenAsync(Guid cinemaId, Guid screenId, Guid seatId, SeatGenerateRequest request);
        Task<BaseResponse<IEnumerable<SeatResponse>>> UpdateSeatStatusesAsync(Guid cinemaId, Guid screenId, IEnumerable<Guid> seatIds);


        Task<BaseResponse<object>> DeleteCinemaAsync(Guid cinemaId);
        Task<BaseResponse<object>> DeleteScreenFromCinemaAsync(Guid cinemaId, Guid screenId);
        Task<BaseResponse<object>> DeleteSeatsFromScreenAsync(Guid cinemaId, Guid screenId, IEnumerable<Guid> seatIds);

    }
}
