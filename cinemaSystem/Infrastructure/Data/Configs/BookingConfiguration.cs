using Domain.Entities.BookingAggregate;
using Domain.Entities.ShowtimeAggregate;
using Domain.Entities.PromotionAggregate;
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

            builder.Property(b => b.BookingCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(b => b.BookingCode)
                .IsUnique();

            builder.Property(b => b.TotalAmount)
                .HasColumnType("decimal(18, 2)");

            builder.Property(b => b.DiscountAmount)
                .HasColumnType("decimal(18, 2)")
                .HasDefaultValue(0);

            builder.Property(b => b.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // FinalStatus mapping: Pending = 0, Confirmed = 1, ...

            builder.Property(b => b.IsCheckedIn)
                .HasDefaultValue(false);

            builder.HasOne<Showtime>()
                .WithMany()
                .HasForeignKey(b => b.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            // CinemaId is optional because some bookings might be made before a showtime is assigned to a cinema
            builder.Property(b => b.CinemaId).IsRequired(false);

            builder.HasOne<Domain.Entities.CinemaAggregate.Cinema>()
                .WithMany()
                .HasForeignKey(b => b.CinemaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<Promotion>()
                .WithMany()
                .HasForeignKey(b => b.PromotionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Accessing private fields for navigation properties
            builder.Navigation(b => b.BookingTickets)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(b => b.Payments)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(b => b.Refunds)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
