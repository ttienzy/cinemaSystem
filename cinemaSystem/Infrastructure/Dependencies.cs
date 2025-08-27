using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Data.Services;
using Infrastructure.ExtenalServices;
using Infrastructure.Identity;
using Infrastructure.Payments.Services;
using Infrastructure.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;


namespace Infrastructure
{
    public class Dependencies
    {
        public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddDbContext<BookingContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("BookingConnection")));
            services.AddDbContext<AppIdentityContext>(optionsAction =>
                optionsAction.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));

            var redisConnection = configuration.GetConnectionString("RedisConnection");
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnection!));

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(ICacheService), typeof(RedisCacheService));
            services.AddTransient(typeof(IEmailService), typeof(EmailService));
            services.AddScoped(typeof(IVnPayService), typeof(VnPayService));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddScoped<ICinemaRepository, CinemaRepository>();
            services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<ISeatTypeService, SeatTypeService>();
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<IPricingTierService, PricingTierService>();
            services.AddScoped<ICinemaService, CinemaService>();
            services.AddScoped<IBookingService, BookingService>();

            services.AddSignalR();
        }
    }
}
