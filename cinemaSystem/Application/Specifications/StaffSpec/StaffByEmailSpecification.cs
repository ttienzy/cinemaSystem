using Ardalis.Specification;
using Domain.Entities.StaffAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.StaffSpec
{
    public class StaffByEmailSpecification : Specification<Staff>
    {
        public StaffByEmailSpecification(string email)
        {
            Query.Where(staff => staff.Email == email).AsNoTracking();
        }
    }
}
