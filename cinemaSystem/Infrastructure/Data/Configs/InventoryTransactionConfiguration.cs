using Domain.Entities.InventoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs
{
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("InventoryTransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(t => t.Note)
                .HasMaxLength(500);

            builder.HasOne<InventoryItem>()
                .WithMany()
                .HasForeignKey(t => t.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
