using Domain.Entities.BookingAggregate;
using Domain.Entities.ShowtimeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.TotalAmount)
                .HasColumnType("decimal(18, 2)");

            builder.Property(b => b.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Pending = 0, Confirmed = 1, Cancelled = 2, Completed = 3
            builder.Property(b => b.IsCheckedIn)
                .HasDefaultValue(false);

            builder.HasOne<Showtime>()
                .WithMany()
                .HasForeignKey(b => b.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Metadata
                .FindNavigation(nameof(Booking.BookingTickets))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata.FindNavigation(nameof(Booking.Payments))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
