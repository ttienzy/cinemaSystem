using Domain.Entities.StaffAggregate;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.Persistence
{
    public interface IStaffRepository
    {
        Task<Staff?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Staff?> GetByIdWithCinemaAsync(Guid id, CancellationToken ct = default);
        Task<Staff?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task AddAsync(Staff staff, CancellationToken ct = default);
        void Update(Staff staff);
        IQueryable<Staff> GetQueryable();
    }
}
