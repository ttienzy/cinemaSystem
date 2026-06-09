
using Booking.API.Infrastructure.Caching.Models;

namespace Booking.API.Infrastructure.Caching.Services;

/// <summary>
/// Service for managing seat status for showtimes.
/// This is the core service that handles seat availability, locking, and booking
/// </summary>
public interface ISeatStatusService
{
    /// <summary>
    /// Get seat availability for a showtime.
    /// </summary>
    Task<SeatAvailabilityResponse> GetSeatAvailabilityAsync(Guid showtimeId);

    /// <summary>
    /// Initialize seat map for a showtime.
    /// </summary>
    Task InitializeSeatMapAsync(Guid showtimeId, Guid cinemaHallId);

    /// <summary>
    /// Seed Redis with seat map data prepared by the application layer.
    /// </summary>
    Task InitializeSeatMapAsync(Guid showtimeId, Guid cinemaHallId, string cinemaHallName, IReadOnlyCollection<SeatStatusDto> seats);

    /// <summary>
    /// Lock seats temporarily for a user (atomic operation)
    /// </summary>
    Task<SeatLockResult> LockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId);

    /// <summary>
    /// Unlock seats (when user deselects or timeout)
    /// </summary>
    Task<bool> UnlockSeatsAsync(Guid showtimeId, List<Guid> seatIds, string userId);

    /// <summary>
    /// Mark seats as booked (after booking created, permanent until cancelled)
    /// </summary>
    Task<bool> MarkSeatsAsBookedAsync(Guid showtimeId, List<Guid> seatIds, Guid bookingId);

    /// <summary>
    /// Release booked seats (when booking cancelled)
    /// </summary>
    Task<bool> ReleaseBookedSeatsAsync(Guid showtimeId, List<Guid> seatIds);

    /// <summary>
    /// Check if seats are available for booking
    /// </summary>
    Task<bool> AreSeatsAvailableAsync(Guid showtimeId, List<Guid> seatIds);

    /// <summary>
    /// Get status of a specific seat
    /// </summary>
    Task<SeatStatusInfo> GetSeatStatusAsync(Guid showtimeId, Guid seatId);

    /// <summary>
    /// Extend lock duration for seats (when user is still in checkout)
    /// </summary>
    Task<bool> ExtendSeatLocksAsync(Guid showtimeId, List<Guid> seatIds, string userId);

    /// <summary>
    /// Verify locked seats and mark as booked (atomic operation for booking confirmation)
    /// This should be used instead of LockSeatsAsync when creating a booking
    /// </summary>
    Task<SeatBookingResult> VerifyAndMarkAsBookedAsync(Guid showtimeId, List<Guid> seatIds, string userId, Guid bookingId);

    /// <summary>
    /// Clean up expired locks for a showtime
    /// </summary>
    Task CleanupExpiredLocksAsync(Guid showtimeId);
}


