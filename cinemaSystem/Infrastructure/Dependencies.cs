using Application.Interfaces.Integrations;
using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Application.Interfaces.Security;
using Infrastructure.BackgroundTasks;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Data.Services;
using Infrastructure.ExtenalServices;
using Infrastructure.Hubs;
using Infrastructure.Identity;
using Infrastructure.Identity.Security;
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

            services.AddTransient(typeof(CleanExpiredReservationWorker));

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
            services.AddScoped<ISeatNotificationService, SeatNotificationService>();

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(ICacheService), typeof(RedisCacheService));
            services.AddTransient(typeof(IEmailService), typeof(EmailService));
            services.AddScoped(typeof(IVnPayService), typeof(VnPayService));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddScoped<ICinemaRepository, CinemaRepository>();
            services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<ISeatTypeService, SeatTypeService>();
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<IPricingTierService, PricingTierService>();
            services.AddScoped<ICinemaService, CinemaService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IShowtimeService, ShowtimeService>();
            services.AddScoped<ITimeSlotService, TimeSlotService>();
            services.AddScoped<IInventoryService, InventoryService>();


            services.AddScoped(typeof(IIdentityUserService), typeof(IdentityService));
            services.AddScoped(typeof(ITokenClaimService), typeof(TokenClaimService));
            services.AddScoped(typeof(IRoleService), typeof(RoleService));
            services.AddScoped(typeof(IOtpService), typeof(OtpService));

        }
    }
}
