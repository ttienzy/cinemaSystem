using Domain.Entities.BookingAggregate;
using Domain.Entities.CinemaAggregate;
using Domain.Entities.ConcessionAggregate;
using Domain.Entities.EquipmentAggregate;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.MovieAggregate;
using Domain.Entities.PromotionAggregate;
using Domain.Entities.SharedAggregates;
using Domain.Entities.ShowtimeAggregate;
using Domain.Entities.StaffAggregate;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DataModels.DashboardDtos.Subs;
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
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<PricingTier> PricingTiers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<CinemaTicketTempModel>().HasNoKey();
            modelBuilder.Entity<ShowtimeOccupancyDto>().HasNoKey();
            modelBuilder.Entity<StaffCheckTempModel>().HasNoKey();

        }
    }
}
