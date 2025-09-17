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
    public class CinemaPublicSpecification : Specification<Cinema, CinemaPublicResponse>
    {
        public CinemaPublicSpecification()
        {
            Query.AsNoTracking()
                .Include(C => C.Screens)
                .Select(e => new CinemaPublicResponse
                {
                    CinemaId = e.Id,
                    Address = e.Address,
                    CinemaName = e.CinemaName,
                    Image = e.Image,
                    Phone = e.Phone,
                    Screens = e.Screens.Count(),
                });
        }
    }
}
