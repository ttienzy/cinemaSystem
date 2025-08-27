using Application.Settings;
using Infrastructure.Hubs;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();


var jwtSettings = builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var smtpSettings = builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
var vnPaySettings = builder.Services.Configure<VnPaySettings>(builder.Configuration.GetSection("VnPay"));
var paymentCallbackSettings = builder.Services.Configure<PaymentCallBackSettings>(builder.Configuration.GetSection("PaymentCallback"));
var timeZoneSettings = builder.Services.Configure<TimeZoneSettings>(builder.Configuration.GetSection("TimeZone"));
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton(smtpSettings);
builder.Services.AddSingleton(vnPaySettings);
builder.Services.AddSingleton(paymentCallbackSettings);
builder.Services.AddSingleton(timeZoneSettings);

Infrastructure.Dependencies.ConfigureServices(builder.Configuration, builder.Services);
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false; // Set to true if you want email confirmation
    options.User.RequireUniqueEmail = true; // Ensure unique email addresses
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
})
    .AddEntityFrameworkStores<AppIdentityContext>()
    .AddDefaultTokenProviders();

Application.Dependencies.AddApplicationDependencies(builder.Services, builder.Configuration);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
})
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder => builder.WithOrigins("http://localhost:5173")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SeatHub>("/seatHub");


app.Run();
