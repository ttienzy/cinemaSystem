using Cys.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Payment.API.Configuration;
using Payment.API.Data;
using Payment.API.Endpoints;
using Payment.API.Integrations.SePay;
using Payment.API.Messaging.EventPublishers;
using Payment.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<PaymentDbContext>("paymentdb");

builder.Services.Configure<SePayOptions>(
    builder.Configuration.GetSection(SePayOptions.SectionName));

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISePayService, SePayService>();
builder.Services.AddScoped<ISePayIpnProcessor, SePayIpnProcessor>();
builder.Services.AddScoped<IPaymentIntegrationEventPublisher, NoOpPaymentIntegrationEventPublisher>();

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? string.Empty;
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? string.Empty;
var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                string.IsNullOrEmpty(jwtKey) ? new byte[32] : Convert.FromBase64String(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));
});

// RabbitMQ/MassTransit is intentionally disabled in this refactor.
// Consumers and the MassTransit publisher are excluded in Payment.API.csproj.

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapPaymentEndpoints();
app.MapSePayIpnEndpoints();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();
