using Domain.Entities.CinemaAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class CinemaConfiguration : IEntityTypeConfiguration<Cinema>
    {
        public void Configure(EntityTypeBuilder<Cinema> builder)
        {
            builder.ToTable("Cinemas");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CinemaName).IsRequired().HasMaxLength(100);
            builder.HasIndex(c => c.CinemaName).IsUnique();
            builder.Property(c => c.Address).IsRequired().HasMaxLength(255);
            builder.Property(c => c.Phone).HasMaxLength(20);
            builder.Property(c => c.Email).HasMaxLength(100);
            builder.Property(c => c.Image).HasMaxLength(500); // Increased from 100
            builder.Property(c => c.ManagerName).HasMaxLength(100);


            builder.Property(c => c.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Open = 0, Closed = 1, UnderMaintenance = 2


            builder.Metadata.FindNavigation(nameof(Cinema.Screens))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
