using CloudinaryDotNet;
using Cinema.API.Client.Extentions;
using Cys.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Movie.API.Clients;
using Movie.API.Data;
using Movie.API.Endpoints;
using Movie.API.Repositories;
using Movie.API.Services;
using Movie.API.Storage.Cloudinary;
using CloudinaryClient = CloudinaryDotNet.Cloudinary;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<MovieDbContext>("moviedb");




builder.Services.Configure<CloudinaryOptions>(
    builder.Configuration.GetSection(CloudinaryOptions.SectionName));
builder.Services.AddSingleton(provider =>
{
    var options = provider.GetRequiredService<IOptions<CloudinaryOptions>>().Value;
    return new CloudinaryClient(new Account(options.CloudName, options.ApiKey, options.ApiSecret));
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IShowtimeService, ShowtimeService>();
builder.Services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
builder.AddCinemaApiClient();
builder.Services.AddHttpClient<IBookingApiClient, BookingApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://booking");
});


var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? string.Empty;
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? string.Empty;
var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;


builder.Services.AddAuthorization();
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
    options.AddPolicy("Admin", p => p.RequireAuthenticatedUser().RequireRole("Admin"));
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapMovieEndpoints();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
    await MovieDbSeeder.SeedAsync(context);
}

app.Run();
