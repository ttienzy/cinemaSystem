using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Features.Concessions.Queries.GetConcessionSales
{
    public record GetConcessionSalesQuery(
        Guid CinemaId,
        DateTime? FromDate,
        DateTime? ToDate,
        int Page = 1,
        int PageSize = 10) : IRequest<ConcessionSalesResponse>;

    public record ConcessionSalesResponse(List<ConcessionSaleDto> Items, int TotalCount);

    public record ConcessionSaleDto(
        Guid Id,
        DateTime SaleDate,
        decimal TotalAmount,
        string PaymentMethod,
        Guid? BookingId,
        int ItemCount);

    public class GetConcessionSalesHandler(IConcessionSaleRepository saleRepo) 
        : IRequestHandler<GetConcessionSalesQuery, ConcessionSalesResponse>
    {
        public async Task<ConcessionSalesResponse> Handle(GetConcessionSalesQuery request, CancellationToken ct)
        {
            var (items, total) = await saleRepo.GetPagedAsync(
                request.CinemaId, 
                request.FromDate, 
                request.ToDate, 
                request.Page, 
                request.PageSize, 
                ct);

            var dtos = items.Select(s => new ConcessionSaleDto(
                s.Id,
                s.SaleDate,
                s.TotalAmount,
                s.PaymentMethod ?? "Unknown",
                s.BookingId,
                s.Items.Count)).ToList();

            return new ConcessionSalesResponse(dtos, total);
        }
    }
}
