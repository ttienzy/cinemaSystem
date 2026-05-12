using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Services;

public class TicketOperationResponseFactory : ITicketOperationResponseFactory
{
    private readonly IExternalServiceClient _externalClient;

    public TicketOperationResponseFactory(IExternalServiceClient externalClient)
    {
        _externalClient = externalClient ?? throw new ArgumentNullException(nameof(externalClient));
    }

    public async Task<TicketOperationResponse> CreateAsync(BookingEntity booking, PaymentLookupDto payment)
    {
        var showtime = await _externalClient.GetShowtimeByIdAsync(booking.ShowtimeId);
        if (showtime == null)
        {
            return booking.MapToTicketOperationResponse(payment, null, []);
        }

        var movieTask = _externalClient.GetMovieByIdAsync(showtime.MovieId);
        var cinemaHallTask = _externalClient.GetCinemaHallByIdAsync(showtime.CinemaHallId);
        var hallSeatsTask = _externalClient.GetSeatsByCinemaHallIdAsync(showtime.CinemaHallId);

        await Task.WhenAll(movieTask, cinemaHallTask, hallSeatsTask);

        var showtimeDetails = TicketOperationMapper.MapToShowtimeDetails(
            showtime,
            await movieTask,
            await cinemaHallTask);

        var seats = booking.MapToBookingSeatResponses(await hallSeatsTask);

        return booking.MapToTicketOperationResponse(payment, showtimeDetails, seats);
    }
}
