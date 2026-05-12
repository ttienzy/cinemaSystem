using Cinema.Shared.Models;

namespace Booking.API.Application.Models;

public sealed class BookingCreationPreparationResult
{
    public ApiResponse<BookingResponse>? FailureResponse { get; init; }
    public ShowtimeDto? Showtime { get; init; }
    public List<SeatDto> SelectedSeats { get; init; } = [];

    public bool Success => FailureResponse == null && Showtime != null;
}
