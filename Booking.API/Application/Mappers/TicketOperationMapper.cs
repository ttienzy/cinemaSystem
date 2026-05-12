using Booking.API.Application.DTOs.External;
using Booking.API.Application.DTOs.Responses;
using BookingSeatResponseDto = Booking.API.Application.DTOs.Responses.BookingSeatDto;
using BookingEntity = Booking.API.Domain.Entities.Booking;

namespace Booking.API.Application.Mappers;

public static class TicketOperationMapper
{
    public static TicketOperationResponse MapToTicketOperationResponse(
        this BookingEntity booking,
        PaymentLookupDto payment,
        ShowtimeDetailsDto? showtimeDetails,
        List<BookingSeatResponseDto> seats)
    {
        return new TicketOperationResponse
        {
            BookingId = booking.Id,
            TicketCode = payment.OrderInvoiceNumber,
            CustomerName = payment.CustomerName,
            CustomerEmail = payment.CustomerEmail,
            CustomerPhone = payment.CustomerPhone,
            BookingStatus = booking.Status,
            PaymentStatus = payment.Status,
            OperationalStatus = MapOperationalStatus(booking.Status, payment.Status),
            CanCheckIn = booking.CanCheckIn(payment.Status),
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            PaidAt = payment.CompletedAt,
            CheckedInAt = booking.Status == BookingStatus.CheckedIn ? booking.UpdatedAt : null,
            Seats = seats,
            ShowtimeDetails = showtimeDetails
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
            CinemaName = "Cinema",
            CinemaHallName = cinemaHall?.Name ?? "Unknown"
        };
    }

    public static List<BookingSeatResponseDto> MapToBookingSeatResponses(
        this BookingEntity booking,
        IEnumerable<SeatDto> hallSeats)
    {
        var seatMap = hallSeats.ToDictionary(seat => seat.Id);

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

    public static bool CanCheckIn(this BookingEntity booking, PaymentLookupStatus paymentStatus)
    {
        return booking.Status == BookingStatus.Confirmed
            && paymentStatus == PaymentLookupStatus.Completed;
    }

    private static string MapOperationalStatus(BookingStatus bookingStatus, PaymentLookupStatus paymentStatus)
    {
        return bookingStatus switch
        {
            BookingStatus.CheckedIn => "CheckedIn",
            BookingStatus.Cancelled => paymentStatus == PaymentLookupStatus.Refunded ? "Refunded" : "Cancelled",
            BookingStatus.Expired => "Expired",
            BookingStatus.Confirmed when paymentStatus == PaymentLookupStatus.Completed => "Paid",
            BookingStatus.Pending when paymentStatus == PaymentLookupStatus.Processing => "ProcessingPayment",
            BookingStatus.Pending when paymentStatus == PaymentLookupStatus.Failed => "PaymentFailed",
            BookingStatus.Pending when paymentStatus == PaymentLookupStatus.Cancelled => "PaymentCancelled",
            _ => bookingStatus.ToString()
        };
    }
}
