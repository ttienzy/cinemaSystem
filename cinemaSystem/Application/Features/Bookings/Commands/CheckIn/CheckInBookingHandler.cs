using Application.Common.Exceptions;
using Application.Common.Interfaces.Persistence;
using Domain.Entities.BookingAggregate.Enums;
using MediatR;

namespace Application.Features.Bookings.Commands.CheckIn
{
    public class CheckInBookingHandler(
        IBookingRepository bookingRepo,
        IShowtimeRepository showtimeRepo,
        IUnitOfWork uow) : IRequestHandler<CheckInBookingCommand, CheckInResult>
    {
        public async Task<CheckInResult> Handle(CheckInBookingCommand cmd, CancellationToken ct)
        {
            // 1. Validate BookingId exists
            var booking = await bookingRepo.GetByIdWithDetailsAsync(cmd.BookingId, ct);
            if (booking == null)
            {
                return new CheckInResult(
                    Success: false,
                    ErrorCode: "BOOKING_NOT_FOUND",
                    ErrorMessage: $"Booking with ID {cmd.BookingId} not found.",
                    Data: null
                );
            }

            // 2. Validate CheckInToken if provided (QR code scan)
            if (!string.IsNullOrEmpty(cmd.CheckInToken))
            {
                if (!string.Equals(booking.CheckInToken, cmd.CheckInToken, StringComparison.OrdinalIgnoreCase))
                {
                    return new CheckInResult(
                        Success: false,
                        ErrorCode: "INVALID_TOKEN",
                        ErrorMessage: "Invalid check-in token. Please scan the correct QR code.",
                        Data: null
                    );
                }
            }

            // 3. Validate booking status is Completed (paid)
            if (booking.Status != BookingStatus.Completed)
            {
                string errorMessage = booking.Status switch
                {
                    BookingStatus.Pending => "Booking has not been paid yet.",
                    BookingStatus.Cancelled => "Booking has been cancelled.",
                    BookingStatus.Refunded => "Booking has been refunded.",
                    BookingStatus.PendingRefund => "Booking is pending refund.",
                    _ => "Booking is not available for check-in."
                };

                return new CheckInResult(
                    Success: false,
                    ErrorCode: "BOOKING_NOT_PAID",
                    ErrorMessage: errorMessage,
                    Data: null
                );
            }

            // 4. Validate booking has not been checked in yet
            if (booking.IsCheckedIn)
            {
                var checkedInTime = booking.CheckedInAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "unknown time";
                return new CheckInResult(
                    Success: false,
                    ErrorCode: "ALREADY_CHECKED_IN",
                    ErrorMessage: $"Ticket has already been used at {checkedInTime}.",
                    Data: null
                );
            }

            // 5. Get showtime details for validation and response
            var showtime = await showtimeRepo.GetByIdAsync(booking.ShowtimeId, ct);
            if (showtime == null)
            {
                return new CheckInResult(
                    Success: false,
                    ErrorCode: "SHOWTIME_NOT_FOUND",
                    ErrorMessage: "Showtime information not found.",
                    Data: null
                );
            }

            // 6. Validate showtime timing (allow check-in from 30 min before to 30 min after start)
            var now = DateTime.UtcNow;
            var showtimeStart = showtime.ActualStartTime;
            var timeDiff = (showtimeStart - now).TotalMinutes;

            if (timeDiff > 30)
            {
                return new CheckInResult(
                    Success: false,
                    ErrorCode: "SHOWTIME_NOT_STARTED",
                    ErrorMessage: $"Showtime has not started yet. Please check in closer to showtime ({showtimeStart:HH:mm}).",
                    Data: null
                );
            }

            if (timeDiff < -30)
            {
                return new CheckInResult(
                    Success: false,
                    ErrorCode: "SHOWTIME_PASSED",
                    ErrorMessage: $"Showtime started at {showtimeStart:HH:mm}, check-in window has passed.",
                    Data: null
                );
            }

            // 7. Perform check-in
            booking.CheckIn();
            await uow.SaveChangesAsync(ct);

            // 8. Build response - Get seats from booking tickets with seat info
            var seats = booking.BookingTickets
                .Where(t => t.Seat != null)
                .Select(t => $"{t.Seat!.RowName}{t.Seat.Number}")
                .ToList();

            // Fallback if seats not loaded
            if (!seats.Any())
            {
                seats = Enumerable.Range(1, booking.TotalTickets)
                    .Select(i => $"Seat-{i}")
                    .ToList();
            }

            return new CheckInResult(
                Success: true,
                ErrorCode: null,
                ErrorMessage: null,
                Data: new BookingCheckInInfo(
                    BookingId: booking.Id,
                    BookingCode: booking.BookingCode,
                    MovieTitle: $"Movie-{booking.ShowtimeId}",
                    ShowTime: showtime.ActualStartTime,
                    CinemaName: $"Cinema-{showtime.CinemaId}",
                    ScreenName: $"Screen-{showtime.ScreenId}",
                    Seats: seats,
                    CheckedInAt: booking.CheckedInAt ?? DateTime.UtcNow
                )
            );
        }
    }
}
