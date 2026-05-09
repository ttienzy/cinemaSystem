using Booking.API.Application.DTOs.Requests;
using Booking.API.Application.DTOs.Responses;
using Cinema.Shared.Models;

namespace Booking.API.Application.Services;

/// <summary>
/// Orchestration service for booking operations
/// Coordinates between external services, repositories, and Redis
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Create a new booking (orchestrates the entire flow)
    /// </summary>
    Task<ApiResponse<BookingResponse>> CreateBookingAsync(CreateBookingRequest request);

    /// <summary>
    /// Get booking by ID with full details
    /// </summary>
    Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(Guid bookingId);

    /// <summary>
    /// Get all bookings for a user
    /// </summary>
    Task<ApiResponse<List<BookingResponse>>> GetUserBookingsAsync(string userId);

    /// <summary>
    /// Cancel a booking
    /// </summary>
    Task<ApiResponse> CancelBookingAsync(Guid bookingId, CancelBookingRequest request);

    /// <summary>
    /// Confirm booking after payment (called by event handler)
    /// </summary>
    Task<ApiResponse> ConfirmBookingAsync(Guid bookingId, string transactionId);

    /// <summary>
    /// Mark booking as expired and release seats
    /// </summary>
    Task<ApiResponse> ExpireBookingAsync(Guid bookingId);
}


