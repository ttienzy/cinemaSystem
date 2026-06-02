using Booking.API.Clients;
using Booking.API.Data;
using Booking.API.Endpoints;
using Booking.API.Infrastructure.Caching.Services;
using Booking.API.Repositories;
using Booking.API.Services;
using Cys.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<BookingDbContext>("bookingdb");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingCreationPreparationService, BookingCreationPreparationService>();
builder.Services.AddScoped<IBookingResponseFactory, BookingResponseFactory>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IBookingAnalyticsService, BookingAnalyticsService>();
builder.Services.AddScoped<ITicketOperationsService, TicketOperationsService>();
builder.Services.AddScoped<ITicketOperationResponseFactory, TicketOperationResponseFactory>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDashboardInsightFactory, DashboardInsightFactory>();
builder.Services.AddScoped<IExternalServiceClient, ExternalServiceClient>();
builder.Services.AddScoped<ISeatStatusService, InMemorySeatStatusService>();
builder.Services.AddHttpClient<CinemaApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://cinema");
});
builder.Services.AddHttpClient<MovieApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://movie");
});
builder.Services.AddHttpClient<PaymentApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://payment");
});



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
    options.AddPolicy("CustomerOrAdmin", policy => policy.RequireAuthenticatedUser().RequireRole("Customer", "Admin"));
});

// Redis is intentionally disabled in this refactor. Booking uses InMemorySeatStatusService for now.
// SignalR hubs are intentionally disabled; hub files are excluded in Booking.API.csproj.
// RabbitMQ/MassTransit consumers and background event publishing are intentionally disabled.

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapSeatAvailabilityEndpoints();
app.MapBookingEndpoints();
app.MapBookingAnalyticsEndpoints();
app.MapBookingOperationsEndpoints();
app.MapDashboardEndpoints();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    await context.Database.MigrateAsync();
}

// SignalR hub mappings are commented out until realtime dependencies are restored.
// app.MapHub<SeatHub>("/hubs/seats");
// app.MapHub<AdminDashboardHub>("/hubs/admin-dashboard");
// app.MapHub<BookingHub>("/hubs/booking");

app.Run();
