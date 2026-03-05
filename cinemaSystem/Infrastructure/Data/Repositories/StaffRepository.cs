using Application.Common.Interfaces.Persistence;
using Domain.Entities.StaffAggregate;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class StaffRepository(BookingContext context) : IStaffRepository
    {
        public async Task<Staff?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await context.Staffs.FindAsync([id], ct);

        public async Task<Staff?> GetByIdWithCinemaAsync(Guid id, CancellationToken ct = default)
            => await context.Staffs
                .Include(s => s.Cinema)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

        public async Task<Staff?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await context.Staffs.FirstOrDefaultAsync(s => s.Email == email, ct);

        public async Task AddAsync(Staff staff, CancellationToken ct = default)
            => await context.Staffs.AddAsync(staff, ct);

        public void Update(Staff staff)
            => context.Staffs.Update(staff);

        public IQueryable<Staff> GetQueryable()
            => context.Staffs.AsQueryable();
    }
}
