// Sử dụng 'global using' hoặc 'using' cho các namespace phổ biến
using Application.Interfaces.Persistences.Repo;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Base;
using Shared.Models.DataModels.DashboardDtos;
using Shared.Models.DataModels.DashboardDtos.Subs;

// Sử dụng file-scoped namespace để giảm thụt lề
namespace Infrastructure.Data.Repositories;

// Đánh dấu lớp là 'sealed' nếu bạn không có ý định kế thừa từ nó, giúp JIT tối ưu hóa.
public sealed class CinemaInsightRepository(BookingContext bookingContext) : ICinemaInsightRepository
{
    // Sử dụng primary constructor và biến thành viên private readonly
    private readonly BookingContext _db = bookingContext;

    // Tối ưu hóa: Gọi một Stored Procedure duy nhất và xử lý kết quả trong C#
    public async Task<StaffStatusSummaryDto> EvaluateDailyManpowerAsync(Guid cinemaId)
    {
        var staffStatusList = await _db.Set<StaffCheckTempModel>()
            .FromSqlInterpolated($"EXEC GetDailyStaffUnifiedReport @CinemaId = {cinemaId}")
            .AsNoTracking()
            .ToListAsync();

        // Xử lý logic tổng hợp trong C# giúp giảm sự phụ thuộc vào database
        return new StaffStatusSummaryDto
        {
            TotalStaffWorkingToday = staffStatusList.Count,
            CheckedIn = staffStatusList.Count(s => s.CheckInStatus == "Checked-In"),
            Late = staffStatusList.Count(s => s.PunctualityStatus == "Late")
        };
    }

    // Tối ưu hóa: Hợp nhất hai phương thức gọi cùng một Stored Procedure
    // Trong lớp CinemaInsightRepository

    public async Task<(TicketReportDto TicketReport, ConcessionReportDto ConcessionReport)> FetchSalesInsightsAsync(CinemaDashboardRequest request)
    {
        // Gọi SP và lấy TOÀN BỘ danh sách kết quả về phía client
        // Stored Procedure của chúng ta được thiết kế để chỉ trả về một hàng duy nhất,
        // vì vậy việc này vẫn rất hiệu quả.
        var salesDataList = await _db.Set<CinemaTicketTempModel>()
            .FromSqlInterpolated($"EXEC GetCinemaReport @CinemaId = {request.CinemaId}, @Year = {request.Year}, @Month = {request.Month}, @Day = {request.Day}")
            .AsNoTracking()
            .ToListAsync(); // <--- THAY ĐỔI QUAN TRỌNG: Lấy danh sách về trước

        // Bây giờ, xử lý danh sách kết quả trong bộ nhớ của C#
        var salesData = salesDataList.FirstOrDefault(); // <--- Lấy phần tử đầu tiên từ danh sách

        // Xử lý trường hợp không có dữ liệu trả về
        if (salesData == null)
        {
            return (new TicketReportDto(), new ConcessionReportDto());
        }

        var ticketReport = new TicketReportDto
        {
            TotalTickets = salesData.TotalTickets,
            TotalTicketsAmount = salesData.TotalTicketsAmount,
            Transactions = salesData.TotalTransactions
        };

        var concessionReport = new ConcessionReportDto
        {
            ItemsSold = salesData.TotalConcessionSales,
            ConcessionRevenue = salesData.TotalAmount
        };

        return (ticketReport, concessionReport);
    }


    // Giữ nguyên phương thức này vì nó đã rất tối ưu và là cách tiếp cận tốt nhất
    public async Task<List<InventoryStatusDto>> ProbeInventorySignalsAsync(Guid cinemaId)
    {
        return await _db.InventoryItems
            .Where(x => x.CinemaId == cinemaId)
            .AsNoTracking() // Move AsNoTracking before Select for clarity
            .Select(x => new InventoryStatusDto
            {
                ItemName = x.ItemName,
                CurrentStock = x.CurrentStock,
                MinimumStock = x.MinimumStock,
                // Replace switch expression with if-else because switch expressions are not supported in expression trees
                StockStatus = x.CurrentStock == 0
                    ? "Hết hàng"
                    : (x.CurrentStock <= x.MinimumStock
                        ? "Cảnh báo: Tồn kho thấp"
                        : "Còn hàng")
            })
            .ToListAsync();
    }

    public async Task<List<ShowtimeOccupancyDto>> ScanShowtimeCapacityAsync(Guid cinemaId)
    {
        // Sử dụng FromSqlInterpolated để an toàn hơn và cú pháp gọn hơn
        return await _db.Set<ShowtimeOccupancyDto>()
            .FromSqlInterpolated($"EXEC GetShowtimeReportByCinema @CinemaId = {cinemaId}")
            .AsNoTracking()
            .ToListAsync();
    }

    #region Các phương thức cũ đã được hợp nhất
    // Hai phương thức này không còn cần thiết vì đã có FetchSalesInsightsAsync
    // public async Task<ConcessionReportDto> FetchSnackInsightsAsync(CinemaDashboardRequest request) { ... }
    // public async Task<TicketReportDto> FetchTicketInsightsAsync(CinemaDashboardRequest request) { ... }
    #endregion
}
