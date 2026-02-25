using Domain.Entities.ConcessionAggregate;
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
    public class ConcessionSaleItemConfiguration : IEntityTypeConfiguration<ConcessionSaleItem>
    {
        public void Configure(EntityTypeBuilder<ConcessionSaleItem> builder)
        {
            builder.ToTable("ConcessionSaleItems");

            builder.HasKey(csi => csi.Id);

            builder.Property(csi => csi.UnitPrice).HasColumnType("decimal(18, 2)");

            // Quan hệ: InventoryItem
            builder.HasOne<InventoryItem>()
                .WithMany()
                .HasForeignKey(csi => csi.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
