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
    public class ScreenConfiguration : IEntityTypeConfiguration<Screen>
    {
        public void Configure(EntityTypeBuilder<Screen> builder)
        {
            builder.ToTable("Screens");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.ScreenName).IsRequired().HasMaxLength(50);

            builder.Property(s => s.Type)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Standard = 0, IMAX = 1, DolbyCinema = 2, ThreeD = 3

            builder.Property(s => s.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Available = 0, UnderMaintenance = 1, Unavailable = 2


            builder.HasOne<Cinema>()
                .WithMany(c => c.Screens)
                .HasForeignKey(s => s.CinemaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(Screen.Seats))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
