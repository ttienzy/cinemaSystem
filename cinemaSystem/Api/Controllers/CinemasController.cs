using Application.Interfaces.Persistences;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.CinemaDtos;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CinemasController : ControllerBase
    {
        private readonly ICinemaService _cinemaService;
        public CinemasController(ICinemaService cinemaService)
        {
            _cinemaService = cinemaService ?? throw new ArgumentNullException(nameof(cinemaService), "Cinema service cannot be null");
        }
        
        [HttpGet]
        public async Task<IActionResult> GetCinemas([FromQuery] CinemaQueryParameters parameters)
        {
            var serviceResponse = await _cinemaService.GetCinemasWithScreensAsync(parameters);
            if (serviceResponse.IsSuccess)
            {
                var pagingList = serviceResponse.Value;
                var result = new
                {
                    Data = pagingList,
                    Pagination = new PaginationResponse
                    {
                        Pageindex = pagingList.PageIndex,
                        TotalPages = pagingList.TotalPages,
                        Count = pagingList.Count,
                        HasNextPage = pagingList.HasNextPage,
                        HasPreviousPage = pagingList.HasPreviousPage,
                    }
                };
                return Ok(result);
            }
            return ErrorReponse<PaginatedList<CinemaResponse>>.WithError(serviceResponse);
        }
        [HttpGet("{cinemaId:guid}/screens/{screenId:guid}/seats")]
        public async Task<IActionResult> GetSeatsForScreen(Guid cinemaId, Guid screenId)
        {
            var serviceResponse = await _cinemaService.GetSeatsForScreenAsync(cinemaId, screenId);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<IEnumerable<SeatResponse>>.WithError(serviceResponse);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCinema([FromBody] CinemaRequest request)
        {
            var serviceResponse = await _cinemaService.CreateCinemaAsync(request);
            if (serviceResponse.IsSuccess)
            {
                return CreatedAtAction(nameof(GetCinemas), new { cinemaId = serviceResponse.Value.Id }, serviceResponse.Value);
            }
            return ErrorReponse<CinemaResponse>.WithError(serviceResponse);
        }
        [HttpPost("{cinemaId:guid}/screens")]
        public async Task<IActionResult> AddScreenToCinema(Guid cinemaId, [FromBody] ScreenRequest request)
        {
            var serviceResponse = await _cinemaService.AddScreenToCinemaAsync(cinemaId, request);
            if (serviceResponse.IsSuccess)
            {
                return CreatedAtAction(nameof(GetSeatsForScreen), new { cinemaId = cinemaId, screenId = serviceResponse.Value.Id }, serviceResponse.Value);
            }
            return ErrorReponse<ScreenResponse>.WithError(serviceResponse);
        }
        [HttpPost("{cinemaId:guid}/screens/{screenId:guid}/seats/generate")]
        public async Task<IActionResult> GenerateSeatsForScreen(Guid cinemaId, Guid screenId, [FromBody] IEnumerable<SeatGenerateRequest> requests)
        {
            var serviceResponse = await _cinemaService.GenerateSeatsForScreenAsync(cinemaId, screenId, requests);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<IEnumerable<SeatResponse>>.WithError(serviceResponse);
        }
        [HttpPut("{cinemaId:guid}")]
        public async Task<IActionResult> UpdateCinema(Guid cinemaId, [FromBody] CinemaRequest request)
        {
            var serviceResponse = await _cinemaService.UpdateCinemaAsync(cinemaId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<CinemaResponse>.WithError(serviceResponse);
        }
        [HttpPut("{cinemaId:guid}/screens/{screenId:guid}")]
        public async Task<IActionResult> UpdateScreenForCinema(Guid cinemaId, Guid screenId, [FromBody] ScreenRequest request)
        {
            var serviceResponse = await _cinemaService.UpdateScreenForCinemaAsync(cinemaId, screenId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<ScreenResponse>.WithError(serviceResponse);
        }
        [HttpPut("{cinemaId:guid}/screens/{screenId:guid}/seats/{seatId:guid}")]
        public async Task<IActionResult> UpdateSeatForScreen(Guid cinemaId, Guid screenId, Guid seatId, [FromBody] SeatGenerateRequest request)
        {
            var serviceResponse = await _cinemaService.UpdateSeatForScreenAsync(cinemaId, screenId, seatId, request);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<SeatResponse>.WithError(serviceResponse);
        }
        [HttpPut("{cinemaId:guid}/screens/{screenId:guid}/seats/updatestatuses")]
        public async Task<IActionResult> UpdateSeatStatuses(Guid cinemaId, Guid screenId, [FromBody] IEnumerable<Guid> seatIds)
        {
            var serviceResponse = await _cinemaService.UpdateSeatStatusesAsync(cinemaId, screenId, seatIds);
            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Value);
            }
            return ErrorReponse<IEnumerable<SeatResponse>>.WithError(serviceResponse);
        }
        [HttpDelete("{cinemaId:guid}")]
        public async Task<IActionResult> DeleteCinema(Guid cinemaId)
        {
            var serviceResponse = await _cinemaService.DeleteCinemaAsync(cinemaId);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{cinemaId:guid}/screens/{screenId:guid}")]
        public async Task<IActionResult> DeleteScreenFromCinema(Guid cinemaId, Guid screenId)
        {
            var serviceResponse = await _cinemaService.DeleteScreenFromCinemaAsync(cinemaId, screenId);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(serviceResponse);
        }
        [HttpDelete("{cinemaId:guid}/screens/{screenId:guid}/seats")]
        public async Task<IActionResult> DeleteSeatsFromScreen(Guid cinemaId, Guid screenId, [FromBody] IEnumerable<Guid> seatIds)
        {
            var serviceResponse = await _cinemaService.DeleteSeatsFromScreenAsync(cinemaId, screenId, seatIds);
            if (serviceResponse.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(serviceResponse);
        }

    }
}
