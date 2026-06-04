using Booking.API.Client;
using Booking.API.Mappers;
using BookingEntity = Booking.API.Entities.Booking;
using ICinemaApiClient = Cinema.API.Client.Client.ICinemaApiClient;
using IMovieApiClient = Movie.API.Client.Client.IMovieApiClient;

namespace Booking.API.Services;

public class BookingResponseFactory : IBookingResponseFactory
{
    private readonly IMovieApiClient _movieApiClient;
    private readonly ICinemaApiClient _cinemaApiClient;

    public BookingResponseFactory(
        IMovieApiClient movieApiClient,
        ICinemaApiClient cinemaApiClient)
    {
        _movieApiClient = movieApiClient ?? throw new ArgumentNullException(nameof(movieApiClient));
        _cinemaApiClient = cinemaApiClient ?? throw new ArgumentNullException(nameof(cinemaApiClient));
    }

    public async Task<BookingResponse> CreateAsync(BookingEntity booking, PaymentCheckoutDto? paymentCheckout = null)
    {
        var showtimeResponse = await _movieApiClient.GetShowtimeByIdAsync(booking.ShowtimeId);
        var showtime = showtimeResponse.Success && showtimeResponse.Data is not null
            ? ExternalClientDtoMapper.ToBookingShowtime(showtimeResponse.Data)
            : null;

        if (showtime == null)
        {
            return booking.MapToBookingResponse(null, [], paymentCheckout);
        }

        var movieTask = _movieApiClient.GetMovieByIdAsync(showtime.MovieId);
        var cinemaHallTask = _cinemaApiClient.GetHallByIdAsync(showtime.CinemaHallId);
        var seatsTask = _cinemaApiClient.GetHallSeatsAsync(showtime.CinemaHallId);

        await Task.WhenAll(movieTask, cinemaHallTask, seatsTask);

        var movieResponse = await movieTask;
        var cinemaHallResponse = await cinemaHallTask;
        var seatsResponse = await seatsTask;

        var showtimeDetails = BookingMapper.MapToShowtimeDetails(
            showtime,
            movieResponse.Success && movieResponse.Data is not null
                ? ExternalClientDtoMapper.ToBookingMovie(movieResponse.Data)
                : null,
            cinemaHallResponse.Success && cinemaHallResponse.Data is not null
                ? ExternalClientDtoMapper.ToBookingCinemaHall(cinemaHallResponse.Data)
                : null);

        var hallSeats = seatsResponse.Success && seatsResponse.Data is not null
            ? seatsResponse.Data.Select(ExternalClientDtoMapper.ToBookingSeat)
            : [];

        var seats = booking.MapToSeatResponses(hallSeats);

        return booking.MapToBookingResponse(showtimeDetails, seats, paymentCheckout);
    }
}
