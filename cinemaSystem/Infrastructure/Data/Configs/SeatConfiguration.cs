using Domain.Entities.CinemaAggregate;
using Domain.Entities.SharedAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class SeatConfiguration : IEntityTypeConfiguration<Seat>
    {
        public void Configure(EntityTypeBuilder<Seat> builder)
        {
            builder.ToTable("Seats");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.RowName)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(s => s.Number)
                .IsRequired();

            builder.Property(s => s.BlockReason)
                .HasMaxLength(200);

            // Unique index to prevent duplicate seats in the same screen
            builder.HasIndex(s => new { s.ScreenId, s.RowName, s.Number })
                .IsUnique();

            builder.HasOne<Screen>()
                .WithMany(s => s.Seats)
                .HasForeignKey(s => s.ScreenId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<SeatType>()
                .WithMany()
                .HasForeignKey(s => s.SeatTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
