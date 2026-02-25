using Domain.Entities.CinemaAggregate;
using Domain.Entities.MovieAggregate;
using Domain.Entities.SharedAggregates;
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
    public class ShowtimeConfiguration : IEntityTypeConfiguration<Showtime>
    {
        public void Configure(EntityTypeBuilder<Showtime> builder)
        {
            builder.ToTable("Showtimes");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Scheduled = 0, OpenForSale = 1, Screening = 2, Finished = 3, Cancelled = 4

            builder.HasIndex(s => s.ShowDate);
            builder.HasIndex(s => new { s.ScreenId, s.ShowDate, s.SlotId }).IsUnique();

            builder.HasOne<Movie>().WithMany().HasForeignKey(s => s.MovieId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Screen>().WithMany().HasForeignKey(s => s.ScreenId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<TimeSlot>().WithMany().HasForeignKey(s => s.SlotId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<PricingTier>().WithMany().HasForeignKey(s => s.PricingTierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Cinema>().WithMany().HasForeignKey(s => s.CinemaId).OnDelete(DeleteBehavior.Restrict);


            builder.Metadata.FindNavigation(nameof(Showtime.ShowtimePricings))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
