using MediatR;
using Shared.Models.DataModels.DashboardDtos;

namespace Application.Features.Dashboard.Queries.GetDashboardSummary
{
    public record GetDashboardSummaryQuery(Guid? CinemaId = null) : IRequest<DashboardSummaryDto>;
}
