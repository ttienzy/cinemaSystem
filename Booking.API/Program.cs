using Cinema.API.Client.Extentions;
using Movie.API.Client.Extentions;
using Payment.API.Client.Extentions;
using Booking.API.Consumers;
using Booking.API.Data;
using Booking.API.Endpoints;
using Booking.API.Hubs;
using Booking.API.Hubs.Services;
using Booking.API.Infrastructure.BackgroundServices;
using Booking.API.Infrastructure.Caching.Services;
using Booking.API.Notifications.Email;
using Booking.API.Repositories;
using Booking.API.Services;
using Cys.ServiceDefaults;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
const string SignalRCorsPolicy = "SignalRClients";

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<BookingDbContext>("bookingdb");


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

builder.Services.AddCors(options =>
{
    options.AddPolicy(SignalRCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:19876",
                "https://localhost:19876",
                "http://127.0.0.1:19876",
                "https://127.0.0.1:19876",
                "http://localhost:19877",
                "https://localhost:19877",
                "http://127.0.0.1:19877",
                "https://127.0.0.1:19877",
                "http://localhost:5000",
                "https://localhost:5000",
                "http://127.0.0.1:5000",
                "https://127.0.0.1:5000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingCreationPreparationService, BookingCreationPreparationService>();
builder.Services.AddScoped<IBookingResponseFactory, BookingResponseFactory>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IBookingAnalyticsService, BookingAnalyticsService>();
builder.Services.AddScoped<ISeatAvailabilityService, SeatAvailabilityService>();
builder.Services.AddScoped<ITicketOperationsService, TicketOperationsService>();
builder.Services.AddScoped<ITicketOperationResponseFactory, TicketOperationResponseFactory>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDashboardInsightFactory, DashboardInsightFactory>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IBookingConfirmationEmailService, BookingConfirmationEmailService>();
builder.Services.AddScoped<IConnectionTracker, RedisConnectionTracker>();
builder.Services.AddSingleton<ISeatNotificationService, SeatNotificationService>();
builder.Services.AddSingleton<IBookingNotificationService, BookingNotificationService>();
builder.Services.AddScoped<IAdminDashboardNotificationService, AdminDashboardNotificationService>();
builder.Services.AddSingleton<IUserIdProvider, ClaimBasedUserIdProvider>();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var redisConnectionString = builder.Configuration.GetConnectionString("redis")
        ?? throw new InvalidOperationException("Redis connection string 'redis' is not configured.");

    return ConnectionMultiplexer.Connect(redisConnectionString);
});
builder.Services.AddScoped<ISeatStatusService, SeatStatusService>();
builder.Services.AddHostedService<SeatLockCleanupService>();
builder.Services.AddHostedService<ExpiredBookingCleanupService>();
builder.AddCinemaApiClient();
builder.AddMovieApiClient();
builder.AddPaymentApiClient();

builder.Services.AddMassTransit(bus =>
{
    bus.AddConsumer<PaymentCompletedConsumer>();
    bus.AddConsumer<PaymentFailedConsumer>();
    bus.AddConsumer<BookingExpiredConsumer>();

    bus.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("booking", false));

    bus.AddConfigureEndpointsCallback((_, _, endpoint) =>
    {
        endpoint.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(2)));

        if (endpoint is IRabbitMqReceiveEndpointConfigurator rabbitEndpoint)
        {
            rabbitEndpoint.Durable = true;
            rabbitEndpoint.AutoDelete = false;
        }
    });

    bus.UsingRabbitMq((context, rabbit) =>
    {
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("RabbitMQ connection string 'rabbitmq' is not configured.");

        rabbit.Host(new Uri(rabbitMqConnectionString));
        rabbit.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(2)));
        rabbit.ConfigureEndpoints(context);
    });
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
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));
    options.AddPolicy("CustomerOrAdmin", policy => policy.RequireAuthenticatedUser().RequireRole("Customer", "Admin"));
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCors(SignalRCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapSeatAvailabilityEndpoints();
app.MapBookingEndpoints();
app.MapBookingAnalyticsEndpoints();
app.MapBookingOperationsEndpoints();
app.MapDashboardEndpoints();
app.MapHub<SeatHub>("/hubs/seats");
app.MapHub<BookingHub>("/hubs/booking");
app.MapHub<AdminDashboardHub>("/hubs/admin-dashboard");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();
