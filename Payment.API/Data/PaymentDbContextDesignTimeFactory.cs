using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Payment.API.Data;

public class PaymentDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__paymentdb")
            ?? "Host=localhost;Database=paymentdb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseNpgsql(connectionString);

        return new PaymentDbContext(optionsBuilder.Options);
    }
}
