using Cinema.Shared.Extensions;
using Payment.API.Api.Endpoints;
using Payment.API.Application;
using Payment.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add database migration
builder.Services.AddMigration<Payment.API.Infrastructure.Persistence.PaymentDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(Payment.API.Infrastructure.DependencyInjection.CorsPolicyName);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPaymentEndpoints();
app.MapSePayIpnEndpoints();
app.MapSePayTestEndpoints();  // ✅ Test endpoints for SePay
app.MapHealthChecks("/health");

app.Run();
