using Ardalis.Specification;
using Domain.Entities.CinemaAggreagte;
using Shared.Models.DataModels.CinemaDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.CinemaSpec
{
    public class CinemaBaseSpecification : Specification<Cinema, CinemaBaseResponse>
    {
        public CinemaBaseSpecification()
        {
            Query.AsNoTracking();
            Query.Select(c => new CinemaBaseResponse
            {
                CinemaId = c.Id,
                Address = c.Address,
                CinemaName = c.CinemaName
            });
        }
    }
}
