using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cinema.API.Data
{
    public class CinemaDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CinemaDbContext>
    {
        public CinemaDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__cinemadb")
                ?? "Host=localhost;Database=cinemadb;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<CinemaDbContext>().UseNpgsql(connectionString);

            return new CinemaDbContext(optionsBuilder.Options);
        }
    }
}
