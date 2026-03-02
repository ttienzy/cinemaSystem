using Domain.Entities.ConcessionAggregate;

namespace Application.Common.Interfaces.Persistence
{
    public interface IConcessionSaleRepository
    {
        Task<ConcessionSale?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ConcessionSale?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
        Task<List<ConcessionSale>> GetByCinemaAndDateAsync(Guid cinemaId, DateTime date, CancellationToken ct = default);
        Task<(List<ConcessionSale> Items, int Total)> GetPagedAsync(
            Guid cinemaId, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default);
        Task AddAsync(ConcessionSale sale, CancellationToken ct = default);
        void Update(ConcessionSale sale);
        IQueryable<ConcessionSale> GetQueryable();
    }
}
