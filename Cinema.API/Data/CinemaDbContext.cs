using CinemaEntity = Cinema.API.Entities.Cinema;
using Microsoft.EntityFrameworkCore;
using Cinema.API.Entities;

namespace Cinema.API.Data;

public class CinemaDbContext : DbContext
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options)
    {
    }

    public DbSet<CinemaEntity> Cinemas { get; set; }
    public DbSet<CinemaHall> CinemaHalls { get; set; }
    public DbSet<Seat> Seats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CinemaHall>()
            .HasOne(ch => ch.Cinema)
            .WithMany(c => c.CinemaHalls)
            .HasForeignKey(ch => ch.CinemaId);

        modelBuilder.Entity<Seat>()
            .HasOne(s => s.CinemaHall)
            .WithMany(ch => ch.Seats)
            .HasForeignKey(s => s.CinemaHallId);

        modelBuilder.Entity<Seat>()
            .HasIndex(s => new { s.CinemaHallId, s.Row, s.Number })
            .IsUnique();

    }
}



