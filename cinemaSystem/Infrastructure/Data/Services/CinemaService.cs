using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.CinemaSpec;
using Domain.Entities.CinemaAggreagte;
using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Common.Paging;
using Shared.Models.DataModels.CinemaDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class CinemaService : ICinemaService
    {
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<SeatType> _seatTypeRepository;
        private readonly ICinemaRepository _cinemaCustomRepository;
        public CinemaService(IRepository<Cinema> cinemaRepository, IRepository<SeatType> seatTypeRepository, ICinemaRepository cinemaCustomRepository)
        {
            _cinemaRepository = cinemaRepository ?? throw new ArgumentNullException(nameof(cinemaRepository), "Cinema repository cannot be null");
            _seatTypeRepository = seatTypeRepository ?? throw new ArgumentNullException(nameof(seatTypeRepository), "Seat type repository cannot be null");
            _cinemaCustomRepository = cinemaCustomRepository ?? throw new ArgumentNullException(nameof(cinemaCustomRepository), "Cinema custom repository cannot be null");
        }
        public async Task<BaseResponse<ScreenResponse>> AddScreenToCinemaAsync(Guid cinemaId, ScreenRequest request)
        {
            try
            {
                var cinemaSpec = new CinemaWithScreensSpecification(cinemaId);
                var cinemaEntity = await _cinemaRepository.FirstOrDefaultAsync(cinemaSpec);
                if (cinemaEntity == null)
                {
                    return BaseResponse<ScreenResponse>.Failure(Error.NotFound($"Cinema with ID {cinemaId} not found"));
                }

                var screen = new Screen(cinemaEntity.Id, request.ScreenName, request.Type, request.Status);
                cinemaEntity.AddItem(screen);
                await _cinemaRepository.UpdateAsync(cinemaEntity);

                ScreenResponse response = new ScreenResponse { Id = screen.Id, ScreenName = screen.ScreenName, Type = screen.Type, Status = screen.Status };
                return BaseResponse<ScreenResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<ScreenResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<CinemaResponse>> CreateCinemaAsync(CinemaRequest request)
        {
            try
            {
                var cinema = new Cinema(request.CinemaName, request.Address, request.Phone, request.Email, request.Image, request.ManagerName, request.Status);
                await _cinemaRepository.AddAsync(cinema);

                var response = new CinemaResponse
                {
                    Id = cinema.Id,
                    Status = cinema.Status,
                    Address = cinema.Address,
                    Phone = cinema.Phone,
                    Email = cinema.Email,
                    Image = cinema.Image,
                    CinemaName = cinema.CinemaName
                };
                return BaseResponse<CinemaResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<CinemaResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteCinemaAsync(Guid cinemaId)
        {
            try
            {
                var cinema = await _cinemaRepository.GetByIdAsync(cinemaId);
                if (cinema == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound("Cinema not found"));
                }
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteScreenFromCinemaAsync(Guid cinemaId, Guid screenId)
        {
            try
            {
                var cinemaWithScreenSpec = new CinemaWithScreensSpecification(cinemaId, screenId);
                var cinemaWithScreen = await _cinemaRepository.FirstOrDefaultAsync(cinemaWithScreenSpec);
                if (cinemaWithScreen == null)
                    return BaseResponse<object>.Failure(Error.NotFound("Screen not found"));

                cinemaWithScreen.RemoveItem();

                await _cinemaRepository.UpdateAsync(cinemaWithScreen);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteSeatsFromScreenAsync(Guid cinemaId, Guid screenId, IEnumerable<Guid> seatIds)
        {
            try
            {
                var cinemaWithScreenSpec = new CinemaScreenAndSeatsSpecification(cinemaId, screenId);
                var cinemaWithScreen = await _cinemaRepository.FirstOrDefaultAsync(cinemaWithScreenSpec);
                if (cinemaWithScreen == null)
                    return BaseResponse<object>.Failure(Error.NotFound("Screen not found"));
                var screen = cinemaWithScreen.Screens.FirstOrDefault(sc => sc.Id == screenId);
                if (screen == null)
                    return BaseResponse<object>.Failure(Error.NotFound("Screen not found"));
                screen.RemoveSeats(seatIds.ToList());
                await _cinemaRepository.UpdateAsync(cinemaWithScreen);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<SeatResponse>>> GenerateSeatsForScreenAsync(Guid cinemaId, Guid screenId, IEnumerable<SeatGenerateRequest> requests)
        {
            try
            {
                var seatTypes = await _seatTypeRepository.ListAsync();
                var cinemaWithScreenSpec = new CinemaScreenAndSeatsSpecification(cinemaId, screenId);
                var cinemaWithScreen = await _cinemaRepository.FirstOrDefaultAsync(cinemaWithScreenSpec);
                if (cinemaWithScreen == null)
                    return BaseResponse<IEnumerable<SeatResponse>>.Failure(Error.NotFound("Screen not found"));

                var screen = cinemaWithScreen.Screens.FirstOrDefault();
                if (screen == null)
                    return BaseResponse<IEnumerable<SeatResponse>>.Failure(Error.NotFound("Screen not found"));

                List<Seat> newSeats = new();
                newSeats.AddRange(requests.Select(r => new Seat( r.SeatTypeId, r.RowName, r.Number, r.IsActive, r.IsBlocked, screenId)));
                screen.AddItems(newSeats);
                await _cinemaRepository.UpdateAsync(cinemaWithScreen);
                var response = newSeats.Select( s => new SeatResponse { Id = s.Id, RowName = s.RowName, Number = s.Number, IsActive = s.IsActive, IsBlocked = s.IsBlocked, SeatTypeName = seatTypes.FirstOrDefault(x => x.Id == s.SeatTypeId)?.TypeName ?? "Unknown" });
                return BaseResponse<IEnumerable<SeatResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<SeatResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<CinemaPublicDetailsResponse>> GetCinemaByIdAsync(Guid cinemaId)
        {
            try
            {
                var cinemaSpec = new CinemaByIdSpecification(cinemaId);
                var cinema = await _cinemaRepository.FirstOrDefaultAsync(cinemaSpec);
                if (cinema == null)
                    return BaseResponse<CinemaPublicDetailsResponse>.Failure(Error.NotFound("Cinema not found"));

                return BaseResponse<CinemaPublicDetailsResponse>.Success(cinema);
            }
            catch (Exception ex)
            {
                return BaseResponse<CinemaPublicDetailsResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<CinemaPublicResponse>>> GetCinemaPublicAsync()
        {
            try
            {
                var cinemaSpec = new CinemaPublicSpecification();
                var cinemas = await _cinemaRepository.ListAsync(cinemaSpec);
                if (cinemas == null)
                {
                    return BaseResponse<IEnumerable<CinemaPublicResponse>>.Success([]);
                }
                return BaseResponse<IEnumerable<CinemaPublicResponse>>.Success(cinemas);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<CinemaPublicResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<PaginatedList<CinemaResponse>>> GetCinemasWithScreensAsync(CinemaQueryParameters parameters)
        {
            try
            {
                var cinemas = _cinemaCustomRepository.GetAllCinemasAsQueryable();
                var pagingCinemas = await PaginatedList<CinemaResponse>.CreateAsync(cinemas, parameters.PageIndex, parameters.PageSize);
                return BaseResponse<PaginatedList<CinemaResponse>>.Success(pagingCinemas);
            }
            catch (Exception ex)
            {
                return BaseResponse<PaginatedList<CinemaResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<SeatResponse>>> GetSeatsForScreenAsync(Guid cinemaId, Guid screenId)
        {
            try
            {
                var seatTypes = await _seatTypeRepository.ListAsync();
                var cinemaWithScreenSpec = new CinemaScreenAndSeatsSpecification(cinemaId, screenId);
                var cinemaWithScreen = await _cinemaRepository.FirstOrDefaultAsync(cinemaWithScreenSpec);

                var screen = cinemaWithScreen?.Screens.FirstOrDefault(sc => sc.Id == screenId);
                if (screen == null)
                    return BaseResponse<IEnumerable<SeatResponse>>.Failure(Error.NotFound("Screen not found"));
                var response = screen.Seats.Select(s => new SeatResponse { 
                    Id = s.Id,
                    RowName = s.RowName,
                    Number = s.Number,
                    IsActive = s.IsActive,
                    IsBlocked = s.IsBlocked,
                    SeatTypeName = seatTypes.FirstOrDefault(x => x.Id == s.SeatTypeId)?.TypeName ?? "Unknown"
                });
                return BaseResponse<IEnumerable<SeatResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<SeatResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<CinemaResponse>> UpdateCinemaAsync(Guid cinemaId, CinemaRequest request)
        {
            try
            {
                var cinema = await _cinemaRepository.GetByIdAsync(cinemaId);
                if (cinema == null)
                {
                    return BaseResponse<CinemaResponse>.Failure(Error.NotFound($"Cinema with ID {cinemaId} not found"));
                }
                cinema.UpdateDetails(request.CinemaName, request.Address, request.Phone, request.Email, request.Image, request.ManagerName, request.Status);

                await _cinemaRepository.UpdateAsync(cinema);
                var response = new CinemaResponse
                {
                    Id = cinema.Id,
                    Status = cinema.Status,
                    Address = cinema.Address,
                    Phone = cinema.Phone,
                    Email = cinema.Email,
                    Image = cinema.Image,
                    CinemaName = cinema.CinemaName
                };
                return BaseResponse<CinemaResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<CinemaResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<ScreenResponse>> UpdateScreenForCinemaAsync(Guid cinemaId, Guid screenId, ScreenRequest request)
        {
            try
            {
                var cinemaSpec = new CinemaWithScreensSpecification(cinemaId, screenId);
                var cinemaEntity = await _cinemaRepository.FirstOrDefaultAsync(cinemaSpec);
                if (cinemaEntity == null)
                {
                    return BaseResponse<ScreenResponse>.Failure(Error.NotFound($"Cinema with ID {cinemaId} not found"));
                }
                var screen = cinemaEntity.Screens.FirstOrDefault(s => s.Id == screenId);
                if (screen == null)
                {
                    return BaseResponse<ScreenResponse>.Failure(Error.NotFound($"Screen with ID {screenId} not found in Cinema {cinemaId}"));
                }
                screen.UpdateDetails(request.ScreenName, request.Type, request.Status);
                await _cinemaRepository.UpdateAsync(cinemaEntity);
                ScreenResponse response = new ScreenResponse { Id = screen.Id, ScreenName = screen.ScreenName, Type = screen.Type, Status = screen.Status };
                return BaseResponse<ScreenResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<ScreenResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<SeatResponse>> UpdateSeatForScreenAsync(Guid cinemaId, Guid screenId, Guid seatId, SeatGenerateRequest request)
        {
            try
            {
                var seatTypes = await _seatTypeRepository.ListAsync();
                var cinemaScreenSpec = new CinemaScreenAndSeatsSpecification(cinemaId, screenId);
                var cinemaEntity = await _cinemaRepository.FirstOrDefaultAsync(cinemaScreenSpec);
                if (cinemaEntity == null)
                {
                    return BaseResponse<SeatResponse>.Failure(Error.NotFound($"Cinema with ID {cinemaId} not found"));
                }
                var screen = cinemaEntity.Screens.FirstOrDefault(s => s.Id == screenId);
                if (screen == null)
                {
                    return BaseResponse<SeatResponse>.Failure(Error.NotFound($"Screen with ID {screenId} not found in Cinema {cinemaId}"));
                }
                screen.UpdateItem(seatId, request.SeatTypeId, request.RowName, request.Number, request.IsActive, request.IsBlocked);
                await _cinemaRepository.UpdateAsync(cinemaEntity);

                var reponse = screen.Seats.Where(s => s.Id == seatId).Select(s => new SeatResponse
                {
                    Id = s.Id,
                    RowName = s.RowName,
                    Number = s.Number,
                    IsActive = s.IsActive,
                    IsBlocked = s.IsBlocked,
                    SeatTypeName = seatTypes.FirstOrDefault(x => x.Id == s.SeatTypeId)?.TypeName ?? "Unknown"
                }).FirstOrDefault();
                return BaseResponse<SeatResponse>.Success(reponse);
            }
            catch (Exception ex)
            {
                return BaseResponse<SeatResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<SeatResponse>>> UpdateSeatStatusesAsync(Guid cinemaId, Guid screenId, IEnumerable<Guid> seatIds)
        {
            try
            {
                var seatTypes = await _seatTypeRepository.ListAsync();
                var cinemaScreenSpec = new CinemaScreenAndSeatsSpecification(cinemaId, screenId);
                var cinemaEntity = await _cinemaRepository.FirstOrDefaultAsync(cinemaScreenSpec);
                if (cinemaEntity == null)
                {
                    return BaseResponse<IEnumerable<SeatResponse>>.Failure(Error.NotFound($"Cinema with ID {cinemaId} not found"));
                }
                var screen = cinemaEntity.Screens.FirstOrDefault(s => s.Id == screenId);
                if (screen == null)
                {
                    return BaseResponse<IEnumerable<SeatResponse>>.Failure(Error.NotFound($"Screen with ID {screenId} not found in Cinema {cinemaId}"));
                }
                screen.IsBlockSeats(seatIds.ToList());
                await _cinemaRepository.UpdateAsync(cinemaEntity);
                var response = screen.Seats.Where(s => seatIds.Contains(s.Id)).Select(s => new SeatResponse
                {
                    Id = s.Id,
                    RowName = s.RowName,
                    Number = s.Number,
                    IsActive = s.IsActive,
                    IsBlocked = s.IsBlocked,
                    SeatTypeName = seatTypes.FirstOrDefault(x => x.Id == s.SeatTypeId)?.TypeName ?? "Unknown"
                });
                return BaseResponse<IEnumerable<SeatResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<SeatResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
