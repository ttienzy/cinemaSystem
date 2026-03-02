using MediatR;

namespace Application.Features.Bookings.Commands.CheckIn
{
    public record CheckInBookingCommand(Guid BookingId, string? CheckInToken = null) : IRequest<CheckInResult>;

    public record CheckInResult(
        bool Success,
        string? ErrorCode,
        string? ErrorMessage,
        BookingCheckInInfo? Data
    );

    public record BookingCheckInInfo(
        Guid BookingId,
        string BookingCode,
        string MovieTitle,
        DateTime ShowTime,
        string CinemaName,
        string ScreenName,
        List<string> Seats,
        DateTime CheckedInAt
    );
}
