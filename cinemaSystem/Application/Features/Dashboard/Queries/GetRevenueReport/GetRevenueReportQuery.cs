using MediatR;
using Shared.Models.DataModels.DashboardDtos;

namespace Application.Features.Dashboard.Queries.GetRevenueReport
{
    /// <summary>
    /// Query for revenue reports — supports filtering by time period, cinema, and grouping by day/week/month.
    /// Admin: view entire chain (cinemaId = null). Manager: view managed cinema.
    /// </summary>
    public record GetRevenueReportQuery(
        DateTime From,
        DateTime To,
        Guid? CinemaId = null,
        string GroupBy = "day" // day | week | month
    ) : IRequest<RevenueReportDto>;
}
