using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Movie.API.Data;

public class MovieDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MovieDbContext>
{
    public MovieDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__moviedb")
            ?? "Host=localhost;Database=moviedb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<MovieDbContext>()
            .UseNpgsql(connectionString);

        return new MovieDbContext(optionsBuilder.Options);
    }
}
