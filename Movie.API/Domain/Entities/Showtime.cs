using Cinema.Shared.Entities;

namespace Movie.API.Domain.Entities;

public class Showtime : BaseEntity
{
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    public Guid CinemaHallId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }

    public static Showtime Create(
        Guid movieId,
        Guid cinemaHallId,
        DateTime startTime,
        int durationMinutes,
        decimal price)
    {
        var showtime = new Showtime
        {
            MovieId = movieId,
            CinemaHallId = cinemaHallId
        };

        showtime.SetSchedule(startTime, durationMinutes, price);
        return showtime;
    }

    public void UpdateSchedule(DateTime startTime, int durationMinutes, decimal price)
    {
        SetSchedule(startTime, durationMinutes, price);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasStarted(DateTime now)
    {
        return StartTime <= now;
    }

    public bool HasBookings(int bookedSeats)
    {
        return bookedSeats > 0;
    }

    public bool CanReschedule(int bookedSeats, DateTime now)
    {
        return !HasBookings(bookedSeats) && !HasStarted(now);
    }

    public DateTime GetCleaningEndTime(int cleaningBufferMinutes)
    {
        return EndTime.AddMinutes(cleaningBufferMinutes);
    }

    public int GetDurationMinutes()
    {
        return (int)(EndTime - StartTime).TotalMinutes;
    }

    public decimal GetOccupancyRate(int bookedSeats, int totalSeats)
    {
        if (totalSeats <= 0)
        {
            return 0;
        }

        return Math.Round((decimal)bookedSeats * 100 / totalSeats, 2);
    }

    private void SetSchedule(DateTime startTime, int durationMinutes, decimal price)
    {
        StartTime = startTime;
        EndTime = startTime.AddMinutes(durationMinutes);
        Price = price;
    }
}


