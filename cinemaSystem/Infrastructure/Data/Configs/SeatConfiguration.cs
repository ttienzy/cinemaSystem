using Domain.Entities.CinemaAggreagte;
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

            builder.Property(s => s.RowName).IsRequired().HasMaxLength(5);

            // Quan hệ: Một SeatType có nhiều Seat (nhưng không có navigation property ngược lại)
            builder.HasOne<SeatType>()
                .WithMany()
                .HasForeignKey(s => s.SeatTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
