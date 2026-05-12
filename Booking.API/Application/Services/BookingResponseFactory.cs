using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Services;

public class BookingResponseFactory : IBookingResponseFactory
{
    private readonly IExternalServiceClient _externalClient;

    public BookingResponseFactory(IExternalServiceClient externalClient)
    {
        _externalClient = externalClient ?? throw new ArgumentNullException(nameof(externalClient));
    }

    public async Task<BookingResponse> CreateAsync(BookingEntity booking, PaymentCheckoutDto? paymentCheckout = null)
    {
        var showtime = await _externalClient.GetShowtimeByIdAsync(booking.ShowtimeId);
        if (showtime == null)
        {
            return booking.MapToBookingResponse(null, [], paymentCheckout);
        }

        var movieTask = _externalClient.GetMovieByIdAsync(showtime.MovieId);
        var cinemaHallTask = _externalClient.GetCinemaHallByIdAsync(showtime.CinemaHallId);
        var seatsTask = _externalClient.GetSeatsByCinemaHallIdAsync(showtime.CinemaHallId);

        await Task.WhenAll(movieTask, cinemaHallTask, seatsTask);

        var showtimeDetails = BookingMapper.MapToShowtimeDetails(
            showtime,
            await movieTask,
            await cinemaHallTask);

        var seats = booking.MapToSeatResponses(await seatsTask);

        return booking.MapToBookingResponse(showtimeDetails, seats, paymentCheckout);
    }
}
