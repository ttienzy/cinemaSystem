using Booking.API.Client;
using BookingSeatResponseDto = Booking.API.Client.BookingSeatDto;
using ClientBookingStatus = Booking.API.Client.BookingStatus;
using DomainBookingStatus = Booking.API.Entities.BookingStatus;
using BookingEntity = Booking.API.Entities.Booking;
using Booking.API.Entities;

namespace Booking.API.Mappers;

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
            BookingStatus = (ClientBookingStatus)booking.Status,
            PaymentStatus = payment.Status,
            OperationalStatus = MapOperationalStatus(booking.Status, payment.Status),
            CanCheckIn = booking.CanCheckIn(payment.Status),
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            PaidAt = payment.CompletedAt,
            CheckedInAt = booking.Status == DomainBookingStatus.CheckedIn ? booking.UpdatedAt : null,
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
        return booking.Status == DomainBookingStatus.Confirmed
            && paymentStatus == PaymentLookupStatus.Completed;
    }

    private static string MapOperationalStatus(DomainBookingStatus bookingStatus, PaymentLookupStatus paymentStatus)
    {
        return bookingStatus switch
        {
            DomainBookingStatus.CheckedIn => "CheckedIn",
            DomainBookingStatus.Cancelled => paymentStatus == PaymentLookupStatus.Refunded ? "Refunded" : "Cancelled",
            DomainBookingStatus.Expired => "Expired",
            DomainBookingStatus.Confirmed when paymentStatus == PaymentLookupStatus.Completed => "Paid",
            DomainBookingStatus.Pending when paymentStatus == PaymentLookupStatus.Processing => "ProcessingPayment",
            DomainBookingStatus.Pending when paymentStatus == PaymentLookupStatus.Failed => "PaymentFailed",
            DomainBookingStatus.Pending when paymentStatus == PaymentLookupStatus.Cancelled => "PaymentCancelled",
            _ => bookingStatus.ToString()
        };
    }
}
