using Booking.API.Client;
using Booking.API.Mappers;
using BookingEntity = Booking.API.Entities.Booking;
using ICinemaApiClient = Cinema.API.Client.Client.ICinemaApiClient;
using IMovieApiClient = Movie.API.Client.Client.IMovieApiClient;

namespace Booking.API.Services;

public class TicketOperationResponseFactory : ITicketOperationResponseFactory
{
    private readonly IMovieApiClient _movieApiClient;
    private readonly ICinemaApiClient _cinemaApiClient;

    public TicketOperationResponseFactory(
        IMovieApiClient movieApiClient,
        ICinemaApiClient cinemaApiClient)
    {
        _movieApiClient = movieApiClient ?? throw new ArgumentNullException(nameof(movieApiClient));
        _cinemaApiClient = cinemaApiClient ?? throw new ArgumentNullException(nameof(cinemaApiClient));
    }

    public async Task<TicketOperationResponse> CreateAsync(BookingEntity booking, PaymentLookupDto payment)
    {
        var showtimeResponse = await _movieApiClient.GetShowtimeByIdAsync(booking.ShowtimeId);
        var showtime = showtimeResponse.Success && showtimeResponse.Data is not null
            ? ExternalClientDtoMapper.ToBookingShowtime(showtimeResponse.Data)
            : null;

        if (showtime == null)
        {
            return booking.MapToTicketOperationResponse(payment, null, []);
        }

        var movieTask = _movieApiClient.GetMovieByIdAsync(showtime.MovieId);
        var cinemaHallTask = _cinemaApiClient.GetHallByIdAsync(showtime.CinemaHallId);
        var hallSeatsTask = _cinemaApiClient.GetHallSeatsAsync(showtime.CinemaHallId);

        await Task.WhenAll(movieTask, cinemaHallTask, hallSeatsTask);

        var movieResponse = await movieTask;
        var cinemaHallResponse = await cinemaHallTask;
        var hallSeatsResponse = await hallSeatsTask;

        var showtimeDetails = TicketOperationMapper.MapToShowtimeDetails(
            showtime,
            movieResponse.Success && movieResponse.Data is not null
                ? ExternalClientDtoMapper.ToBookingMovie(movieResponse.Data)
                : null,
            cinemaHallResponse.Success && cinemaHallResponse.Data is not null
                ? ExternalClientDtoMapper.ToBookingCinemaHall(cinemaHallResponse.Data)
                : null);

        var hallSeats = hallSeatsResponse.Success && hallSeatsResponse.Data is not null
            ? hallSeatsResponse.Data.Select(ExternalClientDtoMapper.ToBookingSeat)
            : [];

        var seats = booking.MapToBookingSeatResponses(hallSeats);

        return booking.MapToTicketOperationResponse(payment, showtimeDetails, seats);
    }
}
