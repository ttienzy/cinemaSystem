using Booking.API.Client;
using CinemaHallClientDto = Cinema.API.Client.CinemaHallDto;
using MovieDetailClientDto = Movie.API.Client.MovieDetailDto;
using MovieLookupClientDto = Movie.API.Client.ShowtimeLookupItemDto;
using PaymentClientDto = Payment.API.Client.PaymentDto;
using PaymentSearchClientDto = Payment.API.Client.PaymentSearchItemResponse;
using SeatClientDto = Cinema.API.Client.SeatDto;
using ShowtimeClientDto = Movie.API.Client.ShowtimeDto;

namespace Booking.API.Services;

internal static class ExternalClientDtoMapper
{
    public static ShowtimeDto ToBookingShowtime(ShowtimeClientDto showtime)
    {
        return new ShowtimeDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            CinemaHallId = showtime.CinemaHallId,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price,
            IsActive = true
        };
    }

    public static MovieDto ToBookingMovie(MovieDetailClientDto movie)
    {
        return new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            DurationMinutes = movie.Duration,
            Genre = string.Join(", ", movie.Genres.Select(genre => genre.Name))
        };
    }

    public static CinemaHallDto ToBookingCinemaHall(CinemaHallClientDto hall)
    {
        return new CinemaHallDto
        {
            Id = hall.Id,
            CinemaId = hall.CinemaId,
            Name = hall.Name,
            TotalSeats = hall.TotalSeats
        };
    }

    public static SeatDto ToBookingSeat(SeatClientDto seat)
    {
        return new SeatDto
        {
            Id = seat.Id,
            CinemaHallId = seat.CinemaHallId,
            Row = seat.Row,
            Number = seat.Number
        };
    }

    public static ShowtimeLookupDto ToBookingShowtimeLookup(MovieLookupClientDto showtime)
    {
        return new ShowtimeLookupDto
        {
            ShowtimeId = showtime.ShowtimeId,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.MovieTitle,
            PosterUrl = showtime.PosterUrl,
            CinemaHallId = showtime.CinemaHallId,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price
        };
    }

    public static PaymentLookupDto ToBookingPayment(PaymentClientDto payment)
    {
        return new PaymentLookupDto
        {
            Id = payment.Id,
            PaymentId = payment.Id,
            BookingId = payment.BookingId,
            OrderInvoiceNumber = payment.OrderInvoiceNumber,
            CustomerEmail = payment.CustomerEmail,
            CustomerPhone = payment.CustomerPhone,
            CustomerName = payment.CustomerName,
            Amount = payment.Amount,
            Status = (PaymentLookupStatus)payment.Status,
            CreatedAt = payment.CreatedAt,
            CompletedAt = payment.CompletedAt
        };
    }

    public static PaymentLookupDto ToBookingPayment(PaymentSearchClientDto payment)
    {
        return new PaymentLookupDto
        {
            Id = payment.PaymentId,
            PaymentId = payment.PaymentId,
            BookingId = payment.BookingId,
            OrderInvoiceNumber = payment.OrderInvoiceNumber,
            CustomerEmail = payment.CustomerEmail,
            CustomerPhone = payment.CustomerPhone,
            CustomerName = payment.CustomerName,
            Amount = payment.Amount,
            Status = (PaymentLookupStatus)payment.Status,
            CreatedAt = payment.CreatedAt,
            CompletedAt = payment.CompletedAt
        };
    }
}
