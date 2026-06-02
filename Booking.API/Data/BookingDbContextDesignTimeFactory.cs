using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Booking.API.Data;

public class BookingDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__bookingdb")
            ?? "Host=localhost;Database=bookingdb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<BookingDbContext>()
            .UseNpgsql(connectionString);

        return new BookingDbContext(optionsBuilder.Options);
    }
}
