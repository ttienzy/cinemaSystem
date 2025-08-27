using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Specifications.BookingSpec;
using Application.Specifications.CinemaSpec;
using Application.Specifications.ShowtimeSpec;
using Domain.Entities.BookingAggregate;
using Domain.Entities.CinemaAggreagte;
using Domain.Entities.CinemaAggreagte.Enum;
using Domain.Entities.MovieAggregate;
using Domain.Entities.SharedAggregates;
using Domain.Entities.ShowtimeAggregate;
using Shared.Common.Base;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly IRepository<Showtime> _showtimeRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IRepository<SeatType> _seatTypeRepository;
        private readonly IRepository<PricingTier> _pricingTierRepository;
        private readonly IRepository<Cinema> _cinemaRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        private readonly IShowtimeRepository _customShowtimeRepository;
        public ShowtimeService
            (IRepository<Showtime> showtimeRepository, 
            IRepository<Booking> bookingRepository,
            IRepository<SeatType> seatTypeRepository,
            IRepository<PricingTier> pricingTierRepository,
            IRepository<Cinema> cinemaRepository,
            IRepository<Movie> movieRepository,
            IRepository<TimeSlot> timeSlotRepository,
            IShowtimeRepository showtimeRepository1)
        {
            _showtimeRepository = showtimeRepository;
            _bookingRepository = bookingRepository;
            _seatTypeRepository = seatTypeRepository;
            _pricingTierRepository = pricingTierRepository;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            _timeSlotRepository = timeSlotRepository;
            _customShowtimeRepository = showtimeRepository1;
        }
        public async Task<BaseResponse<ShowtimePricingResponse>> AddPricingToShowtimeAsync(Guid showtimeId, ShowtimePricingRequest request)
        {
            try
            {
                var showtimeWithPricingSpec = new ShowtimeWithPricingSpecification(showtimeId);
                var showtimeWithPricing = await _showtimeRepository.FirstOrDefaultAsync(showtimeWithPricingSpec);
                if (showtimeWithPricing == null)
                {
                    return BaseResponse<ShowtimePricingResponse>.Failure(Error.NotFound("Showtime not found"));
                }
                var seatType = await _seatTypeRepository.GetByIdAsync(request.SeatTypeId);
                if (seatType == null)
                {
                    return BaseResponse<ShowtimePricingResponse>.Failure(Error.NotFound("Seat type not found"));
                }
                var pricingTier = await _pricingTierRepository.GetByIdAsync(showtimeWithPricing.PricingTierId);
                var newPricing = new ShowtimePricing(request.SeatTypeId, request.BasePrice, Math.Ceiling(request.BasePrice * seatType.PriceMultiplier * pricingTier.Multiplier));
                showtimeWithPricing.AddShowtimePricing(newPricing);
                await _showtimeRepository.UpdateAsync(showtimeWithPricing);
                var response = new ShowtimePricingResponse
                {
                    Id = newPricing.Id,
                    SeatTypeId = newPricing.SeatTypeId,
                    FinalPrice = newPricing.FinalPrice,
                    SeatTypeName = seatType.TypeName
                };
                return BaseResponse<ShowtimePricingResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<ShowtimePricingResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<ShowtimeResponse>> CreateShowtimeAsync(ShowtimeRequest request)
        {

            try
            {
                var movie = await _movieRepository.GetByIdAsync(request.MovieId);
                if (movie == null)
                {
                    return BaseResponse<ShowtimeResponse>.Failure(Error.NotFound("Movie not found"));
                }
                var cinemaScreen = new CinemeWithScreensSpecification(request.CinemaId, request.ScreenId);
                var cinema = await _cinemaRepository.FirstOrDefaultAsync(cinemaScreen);

                if (cinema == null)
                {
                    return BaseResponse<ShowtimeResponse>.Failure(Error.NotFound("Cinema not found"));
                }
                var screen = cinema.Screens.FirstOrDefault();
                if (screen == null)
                {
                    return BaseResponse<ShowtimeResponse>.Failure(Error.NotFound("Screen not found in the specified cinema"));
                }

                var timeSlot = await _timeSlotRepository.GetByIdAsync(request.SlotId);
                if (timeSlot == null)
                {
                    return BaseResponse<ShowtimeResponse>.Failure(Error.NotFound("Time slot not found"));
                }
                var pricingTier = await _pricingTierRepository.GetByIdAsync(request.PricingTierId);
                if (pricingTier == null)
                {
                    return BaseResponse<ShowtimeResponse>.Failure(Error.NotFound("Pricing tier not found"));
                }


                var showtime = new Showtime(request.CinemaId, request.MovieId, request.ScreenId, request.SlotId, request.PricingTierId, request.ShowDate, request.ActualStartTime, request.ActualEndTime, request.Status);
                await _showtimeRepository.AddAsync(showtime);

                var response = new ShowtimeResponse
                {
                    Id = showtime.Id,
                    CinemaId = showtime.CinemaId,
                    MovieId = showtime.MovieId,
                    ScreenId = showtime.ScreenId,
                    SlotId = showtime.SlotId,
                    PricingTierId = showtime.PricingTierId,
                    ShowDate = showtime.ShowDate,
                    ActualStartTime = showtime.ActualStartTime,
                    ActualEndTime = showtime.ActualEndTime,
                    Status = showtime.Status
                };
                return BaseResponse<ShowtimeResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<ShowtimeResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeletePricingFromShowtimeAsync(Guid showtimeId, Guid pricingId)
        {

            try
            {
                var showtimeWithPricingSpec = new ShowtimeWithPricingSpecification(showtimeId);
                var showtimeWithPricing = await _showtimeRepository.FirstOrDefaultAsync(showtimeWithPricingSpec);
                if (showtimeWithPricing == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound("Showtime not found"));
                }
                var result = showtimeWithPricing.RemoveShowtimePricing(pricingId);
                if (!result)
                {
                    return BaseResponse<object>.Failure(Error.NotFound("Pricing not found in the specified showtime"));
                }
                await _showtimeRepository.UpdateAsync(showtimeWithPricing);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<object>> DeleteShowtimeAsync(Guid showtimeId)
        {

            try
            {
                var checkBooingsSpec = new BookingByShowtimeIdSpecification(showtimeId);
                var existingBookings = await _bookingRepository.ListAsync(checkBooingsSpec);
                if (existingBookings.Any())
                {
                    return BaseResponse<object>.Failure(Error.Conflict("Cannot delete showtime with existing bookings"));
                }
                var showtime = await _showtimeRepository.GetByIdAsync(showtimeId);
                if (showtime == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound("Showtime not found"));
                }
                await _showtimeRepository.DeleteAsync(showtime);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<IEnumerable<ShowtimeDetailsResponse>>> GetShowtimesAsync(ShowtimeQueryParameters parameters)
        {

            try
            {
                var showtimes = await _customShowtimeRepository.GetShowtimeByQuerryAsync(parameters.CinemaId, parameters.MovieId, parameters.ShowDate);
                return BaseResponse<IEnumerable<ShowtimeDetailsResponse>>.Success(showtimes);

            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<ShowtimeDetailsResponse>>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<ShowtimeSeatingPlanResponse>> GetShowtimeSeatingPlanAsync(Guid showtimeId)
        {
            try
            {
                // Get showtime with pricings
                var showtimeWithPricingsSpec = new ShowtimeWithPricingSpecification(showtimeId);
                var showtime = await _showtimeRepository.FirstOrDefaultAsync(showtimeWithPricingsSpec);
                if (showtime == null)
                {
                    return BaseResponse<ShowtimeSeatingPlanResponse>.Failure(Error.NotFound("Showtime not found"));
                }

                // Get seat types
                var seatTypes = await _seatTypeRepository.ListAsync();

                // Determine seat status based on bookings
                var bookingSpec = new BookingByShowtimeIdTicketsSpecification(showtimeId);
                var bookings = await _bookingRepository.ListAsync(bookingSpec);

                // Get movie info
                var movie = await _movieRepository.GetByIdAsync(showtime.MovieId);

                // get cinema with screen and seats
                var cinemaScreenWithSeatsSpec = new CinemeWithScreensSpecification(showtime.CinemaId, showtime.ScreenId);
                var cinema = await _cinemaRepository.FirstOrDefaultAsync(cinemaScreenWithSeatsSpec);


                var pricings  = showtime.GetAllShowtimePricings();
                var screen = cinema.Screens.FirstOrDefault();
                var seats = screen.GetAllSeats();
                var bookedSeatIds = bookings.SelectMany(b => b.BookingTickets).Select(t => t.SeatId).ToHashSet();

                var response = new ShowtimeSeatingPlanResponse
                {
                    ShowtimeInfo = new ShowtimeInfoResponse
                    {
                        Id = showtime.Id,
                        ShowDate = showtime.ShowDate,
                        ActualStartTime = showtime.ActualStartTime,
                        ActualEndTime = showtime.ActualEndTime,
                        CinemaName = cinema.CinemaName,
                        MovieTitle = movie.Title,
                        ScreenName = screen.ScreenName
                    },
                    Pricings = pricings.Select(pricing => new ShowtimePricingInfoResponse 
                    { 
                        SeatTypeId = pricing.SeatTypeId,
                        FinalPrice = pricing.FinalPrice,
                    }).ToList(),
                    Seats = seats.Select(seat => new SeatInfoResponse
                    {
                        Id = seat.Id,
                        RowName = seat.RowName,
                        Number = seat.Number,
                        SeatTypeId = seat.SeatTypeId,
                        SeatTypeName = seatTypes.FirstOrDefault(st => st.Id == seat.SeatTypeId)?.TypeName!,
                        Status = bookedSeatIds.Contains(seat.Id) ? SeatStatus.Booked : SeatStatus.Available,
                    }).ToList(),
                };
                return BaseResponse<ShowtimeSeatingPlanResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<ShowtimeSeatingPlanResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<ShowtimePricingResponse>> UpdatePricingToShowtimeAsync(Guid showtimeId, Guid pricingId, ShowtimePricingRequest request)
        {

            try
            {
                var seatType = await _seatTypeRepository.GetByIdAsync(request.SeatTypeId);
                if (seatType == null)
                {
                    return BaseResponse<ShowtimePricingResponse>.Failure(Error.NotFound("Seat type not found"));
                }
                var showtimeWithPricingSpec = new ShowtimeWithPricingSpecification(showtimeId, pricingId);
                var showtimeWithPricing = await _showtimeRepository.FirstOrDefaultAsync(showtimeWithPricingSpec);
                if (showtimeWithPricing == null)
                {
                    return BaseResponse<ShowtimePricingResponse>.Failure(Error.NotFound("Showtime or Pricing not found"));
                }
                var pricingTier = await _pricingTierRepository.GetByIdAsync(showtimeWithPricing.PricingTierId);
                if (pricingTier == null)
                {
                    return BaseResponse<ShowtimePricingResponse>.Failure(Error.NotFound("Pricing tier not found"));
                }
                var showtimePricing = showtimeWithPricing.ShowtimePricings.FirstOrDefault();
                showtimePricing?.UpdatePricing(request.BasePrice, Math.Ceiling(request.BasePrice * seatType.PriceMultiplier * pricingTier.Multiplier));
                await _showtimeRepository.UpdateAsync(showtimeWithPricing);
                var response = new ShowtimePricingResponse
                {
                    Id = showtimePricing.Id,
                    SeatTypeId = showtimePricing.SeatTypeId,
                    FinalPrice = showtimePricing.FinalPrice,
                    SeatTypeName = seatType.TypeName
                };
                return BaseResponse<ShowtimePricingResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<ShowtimePricingResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }

        public async Task<BaseResponse<ShowtimeResponse>> UpdateShowtimeAsync(Guid id, ShowtimeRequest request)
        {

            try
            {
                var showtime = await _showtimeRepository.GetByIdAsync(id);
                if (showtime == null)
                {
                    return BaseResponse<ShowtimeResponse>.Failure(Error.NotFound("Showtime not found"));
                }
                showtime.UpdateShowtime(request.CinemaId, request.MovieId, request.ScreenId, request.SlotId, request.PricingTierId, request.ShowDate, request.ActualStartTime, request.ActualEndTime, request.Status);

                await _showtimeRepository.UpdateAsync(showtime);    
                var response = new ShowtimeResponse
                {
                    Id = showtime.Id,
                    CinemaId = showtime.CinemaId,
                    MovieId = showtime.MovieId,
                    ScreenId = showtime.ScreenId,
                    SlotId = showtime.SlotId,
                    PricingTierId = showtime.PricingTierId,
                    ShowDate = showtime.ShowDate,
                    ActualStartTime = showtime.ActualStartTime,
                    ActualEndTime = showtime.ActualEndTime,
                    Status = showtime.Status
                };
                return BaseResponse<ShowtimeResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<ShowtimeResponse>.Failure(Error.InternalServerError(ex.Message));
            }
        }
    }
}
