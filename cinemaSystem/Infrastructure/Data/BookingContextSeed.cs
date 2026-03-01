using Domain.Entities.SharedAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public static class BookingContextSeed
    {
        public static async Task SeedAsync(BookingContext context, ILogger logger)
        {
            try
            {
                if (context.Database.IsSqlServer())
                {
                    await context.Database.MigrateAsync();
                }

                await SeedGenresAsync(context, logger);
                await SeedSeatTypesAsync(context, logger);
                await SeedTimeSlotsAsync(context, logger);
                await SeedPricingTiersAsync(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi xảy ra trong quá trình seed dữ liệu cho BookingContext");
            }
        }

        private static async Task SeedGenresAsync(BookingContext context, ILogger logger)
        {
            if (!await context.Set<Genre>().AnyAsync())
            {
                var genres = new List<Genre>
                {
                    new Genre("Action", "Phim Hành Động"),
                    new Genre("Comedy", "Phim Hài Hước"),
                    new Genre("Drama", "Phim Tâm Lý Tình Cảm"),
                    new Genre("Horror", "Phim Kinh Dị"),
                    new Genre("Sci-Fi", "Phim Khoa Học Viễn Tưởng"),
                    new Genre("Romance", "Phim Lãng Mạn"),
                    new Genre("Animation", "Phim Hoạt Hình")
                };

                await context.Set<Genre>().AddRangeAsync(genres);
                await context.SaveChangesAsync();
                logger.LogInformation("Đã seed thành công dữ liệu mẫu cho Thể loại phim (Genres).");
            }
        }

        private static async Task SeedSeatTypesAsync(BookingContext context, ILogger logger)
        {
            if (!await context.Set<SeatType>().AnyAsync())
            {
                var seatTypes = new List<SeatType>
                {
                    new SeatType("Standard", 1.0m),
                    new SeatType("VIP", 1.5m),
                    new SeatType("Couple", 2.2m),
                    new SeatType("Wheelchair", 1.0m)
                };

                await context.Set<SeatType>().AddRangeAsync(seatTypes);
                await context.SaveChangesAsync();
                logger.LogInformation("Đã seed thành công dữ liệu mẫu cho Loại ghế (SeatTypes).");
            }
        }

        private static async Task SeedTimeSlotsAsync(BookingContext context, ILogger logger)
        {
            if (!await context.Set<TimeSlot>().AnyAsync())
            {
                var timeSlots = new List<TimeSlot>
                {
                    new TimeSlot(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), "Weekday", true),
                    new TimeSlot(new TimeSpan(12, 0, 0), new TimeSpan(17, 0, 0), "Weekday", true),
                    new TimeSlot(new TimeSpan(17, 0, 0), new TimeSpan(23, 59, 59), "Weekday", true),
                    new TimeSlot(new TimeSpan(8, 0, 0), new TimeSpan(23, 59, 59), "Weekend", true)
                };

                await context.Set<TimeSlot>().AddRangeAsync(timeSlots);
                await context.SaveChangesAsync();
                logger.LogInformation("Đã seed thành công dữ liệu mẫu cho Khung giờ (TimeSlots).");
            }
        }

        private static async Task SeedPricingTiersAsync(BookingContext context, ILogger logger)
        {
            if (!await context.Set<PricingTier>().AnyAsync())
            {
                var pricingTiers = new List<PricingTier>
                {
                    new PricingTier("Base Rate", 1.0m, "Mon,Tue,Wed,Thu"),
                    new PricingTier("Weekend Surge", 1.25m, "Fri,Sat,Sun"),
                    new PricingTier("Holiday Special", 1.5m, "Holiday"),
                    new PricingTier("Student Discount", 0.8m, "Mon,Tue,Wed")
                };

                await context.Set<PricingTier>().AddRangeAsync(pricingTiers);
                await context.SaveChangesAsync();
                logger.LogInformation("Đã seed thành công dữ liệu mẫu cho Bậc giá (PricingTiers).");
            }
        }
    }
}
