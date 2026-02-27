using Domain.Entities.BookingAggregate;
using Domain.Entities.CinemaAggregate;
using Domain.Entities.ConcessionAggregate;
using Domain.Entities.StaffAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class ConcessionSaleConfiguration : IEntityTypeConfiguration<ConcessionSale>
    {
        public void Configure(EntityTypeBuilder<ConcessionSale> builder)
        {
            builder.ToTable("ConcessionSales");

            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.TotalAmount).HasColumnType("decimal(18, 2)");
            builder.Property(cs => cs.PaymentMethod).IsRequired().HasMaxLength(50);

            builder.HasOne<Cinema>()
                .WithMany()
                .HasForeignKey(cs => cs.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan h?: Booking (optional)
            builder.HasOne<Booking>()
                .WithMany()
                .HasForeignKey(cs => cs.BookingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<Staff>()
                .WithMany()
                .HasForeignKey(cs => cs.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(cs => cs.Items)
                .WithOne()
                .HasForeignKey(csi => csi.ConcessionSaleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(ConcessionSale.Items))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
