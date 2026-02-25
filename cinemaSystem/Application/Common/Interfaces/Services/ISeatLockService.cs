namespace Application.Common.Interfaces.Services
{
    /// <summary>
    /// Redis-backed seat locking service.
    /// Locks are set when a booking is created (15-min TTL) and
    /// released when booking is completed, cancelled, or expired.
    /// </summary>
    public interface ISeatLockService
    {
        /// <summary>Lock seats for a specific booking with TTL.</summary>
        Task LockSeatsAsync(Guid showtimeId, List<Guid> seatIds, Guid bookingId, TimeSpan ttl, CancellationToken ct = default);

        /// <summary>Release locked seats (called on cancel/complete/expire).</summary>
        Task ReleaseSeatsAsync(Guid showtimeId, List<Guid> seatIds, CancellationToken ct = default);

        /// <summary>Get the current lock status for each seat in a showtime.</summary>
        Task<Dictionary<Guid, SeatLockStatus>> GetSeatStatusesAsync(Guid showtimeId, List<Guid> seatIds, CancellationToken ct = default);
    }

    public enum SeatLockStatus
    {
        Available,
        Locked,    // Locked in Redis (someone has it in a pending booking)
        Booked     // Confirmed in DB
    }
}
