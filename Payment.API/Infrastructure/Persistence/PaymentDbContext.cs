using Microsoft.EntityFrameworkCore;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<PaymentEntity> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Payment entity configuration
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasIndex(p => p.BookingId);
            entity.HasIndex(p => p.OrderInvoiceNumber).IsUnique();
            entity.HasIndex(p => p.TransactionId).IsUnique().HasFilter("[TransactionId] IS NOT NULL");
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.CreatedAt);

            entity.Property(p => p.OrderInvoiceNumber).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Currency).HasMaxLength(10).IsRequired().HasDefaultValue("VND");
            entity.Property(p => p.OrderDescription).HasMaxLength(500).IsRequired();
            entity.Property(p => p.PaymentGateway).HasMaxLength(50).IsRequired().HasDefaultValue("SePay");
            entity.Property(p => p.TransactionId).HasMaxLength(100);
            entity.Property(p => p.PaymentMethod).HasMaxLength(50);
            entity.Property(p => p.CustomerEmail).HasMaxLength(100).IsRequired();
            entity.Property(p => p.CustomerPhone).HasMaxLength(20).IsRequired();
            entity.Property(p => p.SuccessUrl).HasMaxLength(500);
            entity.Property(p => p.ErrorUrl).HasMaxLength(500);
            entity.Property(p => p.CancelUrl).HasMaxLength(500);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

    }
}



