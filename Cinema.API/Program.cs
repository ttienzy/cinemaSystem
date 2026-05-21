using Cinema.API.Api.Endpoints;
using Cinema.API.Application;
using Cinema.API.Infrastructure;
using Cinema.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Configure JSON serialization to use camelCase
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (args.IsMigrationOnlyCommand())
{
    await app.MigrateAndStopAsync<Cinema.API.Infrastructure.Persistence.CinemaDbContext>();
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapCinemaEndpoints();
app.MapCinemaHallEndpoints();
app.MapSeatEndpoints();
app.MapHealthChecks("/health");

app.Run();
