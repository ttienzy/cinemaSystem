using Cinema.API;
using Cinema.API.Data;
using Cinema.API.Endpoints;
using Cinema.API.Repositories;
using Cinema.API.Services;
using Cys.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CinemaDbContext>("cinemadb");

builder.Services.AddScoped<ICinemaRepository, CinemaRepository>();
builder.Services.AddScoped<ICinemaHallRepository, CinemaHallRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<ICinemaService, CinemaService>();
builder.Services.AddScoped<ICinemaHallService, CinemaHallService>();
builder.Services.AddScoped<ISeatService, SeatService>();

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
            NameClaimType = "name",
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

app.MapCinemaEndpoints();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
    await CinemaDbSeeder.SeedAsync(context);
}



app.Run();
