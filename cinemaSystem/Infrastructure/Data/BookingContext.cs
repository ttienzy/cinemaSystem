using Domain.Entities.BookingAggregate;
using Domain.Entities.CinemaAggreagte;
using Domain.Entities.ConcessionAggregate;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.MovieAggregate;
using Domain.Entities.SharedAggregates;
using Domain.Entities.ShowtimeAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class BookingContext : DbContext
    {
        public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<MovieCertification> MovieCertifications { get; set; }
        public DbSet<MovieCopyright> MovieCopyrights { get; set; }
        public DbSet<MovieCastCrew> MovieCastCrews { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<SeatType> SeatTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingTicket> BookingTickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<ShowtimePricing> ShowtimePricings { get; set; }
        public DbSet<ConcessionSale> ConcessionSales { get; set; }
        public DbSet<ConcessionSaleItem> ConcessionSaleItems { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
