using Cinema.Shared.Extensions;
using Movie.API.Api.Endpoints;
using Movie.API.Application;
using Movie.API.Infrastructure;

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
    await app.MigrateAndStopAsync<Movie.API.Infrastructure.Persistence.MovieDbContext>();
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

app.MapMovieEndpoints();
app.MapGenreEndpoints();
app.MapShowtimeEndpoints();
app.MapHealthChecks("/health");

app.Run();
