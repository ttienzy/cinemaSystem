using Domain.Entities.InventoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configs
{
    public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.ToTable("InventoryItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ItemName).IsRequired().HasMaxLength(150);
            builder.Property(i => i.ItemCategory).HasMaxLength(100);
            builder.Property(i => i.SupplierInfo).HasMaxLength(255);

            builder.Property(i => i.UnitPrice).HasColumnType("decimal(18, 2)");
            builder.Property(i => i.CostPrice).HasColumnType("decimal(18, 2)");

            builder.HasOne<Domain.Entities.CinemaAggreagte.Cinema>()
                .WithMany()
                .HasForeignKey(i => i.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
