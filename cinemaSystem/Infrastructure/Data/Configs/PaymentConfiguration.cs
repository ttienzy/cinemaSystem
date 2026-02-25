using Domain.Entities.BookingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PaymentMethod).IsRequired(false).HasMaxLength(50);
            builder.Property(p => p.PaymentProvider).HasMaxLength(50);
            builder.Property(p => p.Currency).IsRequired(false).HasMaxLength(10);
            builder.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>(); // Pending = 0, Completed = 1, Failed = 2, Refunded = 3

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18, 2)");

            builder.HasIndex(p => p.TransactionId)
                .IsUnique()
                .HasFilter("[TransactionId] IS NOT NULL");
        }
    }
}
