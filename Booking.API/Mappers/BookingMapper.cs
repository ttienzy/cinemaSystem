using Booking.API.Client;
using BookingSeatResponseDto = Booking.API.Client.BookingSeatDto;
using ClientBookingStatus = Booking.API.Client.BookingStatus;
using BookingEntity = Booking.API.Entities.Booking;

namespace Booking.API.Mappers;

public static class BookingMapper
{
    public static BookingResponse MapToBookingResponse(
        this BookingEntity booking,
        ShowtimeDetailsDto? showtimeDetails,
        List<BookingSeatResponseDto> seats,
        PaymentCheckoutDto? paymentCheckout = null)
    {
        return new BookingResponse
        {
            BookingId = booking.Id,
            UserId = booking.UserId,
            ShowtimeId = booking.ShowtimeId,
            Status = (ClientBookingStatus)booking.Status,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            ExpiresAt = booking.ExpiresAt,
            Seats = seats,
            ShowtimeDetails = showtimeDetails,
            PaymentId = paymentCheckout?.PaymentId,
            CheckoutUrl = paymentCheckout?.CheckoutUrl
        };
    }

    public static ShowtimeDetailsDto MapToShowtimeDetails(
        this ShowtimeDto showtime,
        MovieDto? movie,
        CinemaHallDto? cinemaHall)
    {
        return new ShowtimeDetailsDto
        {
            ShowtimeId = showtime.Id,
            MovieTitle = movie?.Title ?? "Unknown",
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            CinemaHallName = cinemaHall?.Name ?? "Unknown"
        };
    }

    public static List<BookingSeatResponseDto> MapToSeatResponses(
        this BookingEntity booking,
        IEnumerable<SeatDto> seats)
    {
        var seatMap = seats.ToDictionary(seat => seat.Id);

        return booking.BookingSeats
            .Select(bookingSeat =>
            {
                if (!seatMap.TryGetValue(bookingSeat.SeatId, out var seat))
                {
                    return null;
                }

                return new BookingSeatResponseDto
                {
                    SeatId = seat.Id,
                    Row = seat.Row,
                    Number = seat.Number,
                    Price = bookingSeat.Price
                };
            })
            .Where(seat => seat != null)
            .Cast<BookingSeatResponseDto>()
            .ToList();
    }
}
