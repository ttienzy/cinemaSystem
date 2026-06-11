using BookingEntity = Booking.API.Entities.Booking;
using Microsoft.EntityFrameworkCore;
using Booking.API.Entities;

namespace Booking.API.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    public DbSet<BookingEntity> Bookings { get; set; }
    public DbSet<BookingSeat> BookingSeats { get; set; }
    // SeatLock removed - Redis handles seat locking

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BookingSeat>()
            .HasOne(bs => bs.Booking)
            .WithMany(b => b.BookingSeats)
            .HasForeignKey(bs => bs.BookingId);

        // SeatLock table removed - Redis handles locking

        modelBuilder.Entity<BookingEntity>()
            .Property(b => b.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BookingSeat>()
            .Property(bs => bs.Price)
            .HasPrecision(18, 2);
    }
}



