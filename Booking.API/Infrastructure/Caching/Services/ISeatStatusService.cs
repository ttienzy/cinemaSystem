using Booking.API.Application.DTOs.Responses;

namespace Booking.API.Infrastructure.Caching.Services;

/// <summary>
/// Service for managing seat status in Redis for showtimes
/// This is the core service that handles seat availability, locking, and booking
/// </summary>
public interface ISeatStatusService
{
    /// <summary>
    /// Get seat availability for a showtime (aggregates data from Cinema.API + Redis)
    /// </summary>
    Task<SeatAvailabilityResponse> GetSeatAvailabilityAsync(Guid showtimeId);

    /// <summary>
    /// Initialize seat map in Redis for a showtime (first time access)
    /// </summary>
    Task InitializeSeatMapAsync(Guid showtimeId, Guid cinemaHallId);

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

/// <summary>
/// Result of seat locking operation
/// </summary>
public class SeatLockResult
{
    public bool Success { get; set; }
    public List<Guid> LockedSeats { get; set; } = new();
    public List<Guid> AlreadyLockedSeats { get; set; } = new();
    public string? Message { get; set; }
}

/// <summary>
/// Result of verify and mark as booked operation
/// </summary>
public class SeatBookingResult
{
    public bool Success { get; set; }
    public List<Guid> BookedSeats { get; set; } = new();
    public List<Guid> FailedSeats { get; set; } = new();
    public string? Message { get; set; }
    public SeatBookingFailureReason? FailureReason { get; set; }
}

/// <summary>
/// Reasons why seat booking verification failed
/// </summary>
public enum SeatBookingFailureReason
{
    NotLocked,          // Seat is not locked at all
    LockExpired,        // Lock has expired
    WrongUser,          // Locked by different user
    AlreadyBooked,      // Seat is already booked
    Unavailable         // Seat doesn't exist or other error
}

/// <summary>
/// Detailed seat status information
/// </summary>
public class SeatStatusInfo
{
    public Guid SeatId { get; set; }
    public SeatStatus Status { get; set; }
    public string? UserId { get; set; }
    public Guid? BookingId { get; set; }
    public DateTime? LockedUntil { get; set; }
}


