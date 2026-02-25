using Domain.Entities.PromotionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.ToTable("Promotions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(p => p.Code)
                .IsUnique();

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Value)
                .HasColumnType("decimal(18, 2)");

            builder.Property(p => p.MaxDiscountAmount)
                .HasColumnType("decimal(18, 2)");

            builder.Property(p => p.MinOrderValue)
                .HasColumnType("decimal(18, 2)");

            builder.Property(p => p.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);
        }
    }
}
