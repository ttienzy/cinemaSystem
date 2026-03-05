using MediatR;
using Shared.Models.DataModels.DashboardDtos;

namespace Application.Features.Dashboard.Queries.GetRevenueReport
{
    /// <summary>
    /// Truy vấn báo cáo doanh thu — hỗ trợ lọc theo khoảng thời gian, rạp, và nhóm theo ngày/tuần/tháng.
    /// Admin: xem toàn chuỗi (cinemaId = null). Manager: xem rạp mình quản lý.
    /// </summary>
    public record GetRevenueReportQuery(
        DateTime From,
        DateTime To,
        Guid? CinemaId = null,
        string GroupBy = "day" // day | week | month
    ) : IRequest<RevenueReportDto>;
}
