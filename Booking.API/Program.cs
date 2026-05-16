using Booking.API.Api.Endpoints;
using Booking.API.Application;
using Booking.API.Infrastructure;
using Booking.API.Infrastructure.Hubs;
using Booking.API.Infrastructure.Serialization;
using Cinema.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON serialization to use camelCase
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new DateTimeUtcConverter());
    options.SerializerOptions.Converters.Add(new NullableDateTimeUtcConverter());
});

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

// Add database migration
builder.Services.AddMigration<Booking.API.Infrastructure.Persistence.BookingDbContext>();

// Configure CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000", "http://localhost:4200" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for SignalR
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("SignalRCorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map Endpoints
app.MapSeatAvailabilityEndpoints();
app.MapBookingEndpoints();
app.MapBookingAnalyticsEndpoints();
app.MapBookingOperationsEndpoints();
app.MapDashboardEndpoints();
app.MapHealthChecks("/health");

// Map SignalR Hub
app.MapHub<SeatHub>("/hubs/seats");
app.MapHub<AdminDashboardHub>("/hubs/admin-dashboard");
app.MapHub<BookingHub>("/hubs/booking");

app.Run();
