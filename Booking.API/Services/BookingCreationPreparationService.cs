using Booking.API.Exceptions;
using Booking.API.Models;
using Booking.API.Client;
using ICinemaApiClient = Cinema.API.Client.Client.ICinemaApiClient;
using IMovieApiClient = Movie.API.Client.Client.IMovieApiClient;
using Booking.API.Mappers;

namespace Booking.API.Services;

public class BookingCreationPreparationService : IBookingCreationPreparationService
{
    private readonly IMovieApiClient _movieApiClient;
    private readonly ICinemaApiClient _cinemaApiClient;
    private readonly IConfiguration _configuration;

    public BookingCreationPreparationService(
        IMovieApiClient movieApiClient,
        ICinemaApiClient cinemaApiClient,
        IConfiguration configuration)
    {
        _movieApiClient = movieApiClient ?? throw new ArgumentNullException(nameof(movieApiClient));
        _cinemaApiClient = cinemaApiClient ?? throw new ArgumentNullException(nameof(cinemaApiClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<BookingCreationPreparationResult> PrepareAsync(CreateBookingRequest request)
    {
        var validationErrors = ValidateSeatSelection(request.SeatIds);
        if (validationErrors.Count > 0)
        {
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.ValidationErrorResponse(
                    BookingException.VALIDATION_FAILED,
                    validationErrors)
            };
        }

        var showtimeResponse = await _movieApiClient.GetShowtimeByIdAsync(request.ShowtimeId);
        var showtime = showtimeResponse.Success && showtimeResponse.Data is not null
            ? ExternalClientDtoMapper.ToBookingShowtime(showtimeResponse.Data)
            : null;

        if (showtime == null)
        {
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.NotFoundResponse(
                    BookingException.SHOWTIME_NOT_FOUND(request.ShowtimeId))
            };
        }

        if (!showtime.IsActive || showtime.StartTime < DateTime.UtcNow)
        {
            var value = BookingException.SHOWTIME_INACTIVE;
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.FailureResponse(
                    BookingException.SHOWTIME_NOT_AVAILABLE,
                    400,
                    [new ErrorDetail(value.Code, value.Message, value.Field)])
            };
        }

        var timingErrors = ValidateShowtimeTiming(showtime);
        if (timingErrors.Count > 0)
        {
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.ValidationErrorResponse(
                    BookingException.VALIDATION_FAILED,
                    timingErrors)
            };
        }

        var seatResponse = await _cinemaApiClient.GetHallSeatsAsync(showtime.CinemaHallId);
        var seats = seatResponse.Success && seatResponse.Data is not null
            ? seatResponse.Data.Select(ExternalClientDtoMapper.ToBookingSeat).ToList()
            : [];

        var selectedSeats = seats
            .Where(seat => request.SeatIds.Contains(seat.Id))
            .ToList();

        if (selectedSeats.Count != request.SeatIds.Count)
        {
            var value = BookingException.INVALID_SEATS;
            return new BookingCreationPreparationResult
            {
                FailureResponse = ApiResponse<BookingResponse>.FailureResponse(
                    BookingException.INVALID_SEATS_MESSAGE,
                    400,
                    [new ErrorDetail(value.Code, value.Message, value.Field)])
            };
        }

        return new BookingCreationPreparationResult
        {
            Showtime = showtime,
            SelectedSeats = selectedSeats
        };
    }

    private List<ErrorDetail> ValidateSeatSelection(List<Guid> seatIds)
    {
        var errors = new List<ErrorDetail>();
        var maxSeats = _configuration.GetValue<int>("Booking:MaxSeatsPerBooking", 10);

        if (seatIds.Count > maxSeats)
        {
            var value = BookingException.MAX_SEATS_EXCEEDED(maxSeats);
            errors.Add(new ErrorDetail(value.Code, value.Message, value.Field));
        }

        if (seatIds.Count == 0)
        {
            var value = BookingException.SEATS_REQUIRED;
            errors.Add(new ErrorDetail(value.Code, value.Message, value.Field));
        }

        return errors;
    }

    private List<ErrorDetail> ValidateShowtimeTiming(ShowtimeDto showtime)
    {
        var errors = new List<ErrorDetail>();
        var minMinutes = _configuration.GetValue<int>("Booking:MinutesBeforeShowtimeToBook", 30);
        var minBookingTime = showtime.StartTime.AddMinutes(-minMinutes);

        if (DateTime.UtcNow > minBookingTime)
        {
            var value = BookingException.TOO_LATE_TO_BOOK(minMinutes);
            errors.Add(new ErrorDetail(value.Code, value.Message, value.Field));
        }

        return errors;
    }
}
