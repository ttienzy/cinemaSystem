using Application.Interfaces.Persistences.Repo;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class EfRepository<T> : RepositoryBase<T>, IRepository<T> where T : class, IAggregateRoot
    {
        public EfRepository(BookingContext bookingContext) : base(bookingContext)
        {
        }
    }
}
