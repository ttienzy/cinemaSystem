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
    public class CinemaByIdSpecification : Specification<Cinema, CinemaPublicDetailsResponse>
    {
        public CinemaByIdSpecification(Guid cinemaId)
        {
            Query.AsNoTracking()
                .Where(c => c.Id == cinemaId)
                .Include(sc => sc.Screens).ThenInclude(s => s.Seats)
                .Select(e => new CinemaPublicDetailsResponse
                {
                    Cinema = new CinemaDetails
                    {
                        Address = e.Address,
                        CinemaName = e.CinemaName,
                        Email = e.Email,
                        Image = e.Email,
                        ManagerName = e.ManagerName,
                        Phone = e.Phone,
                        Status = e.Status
                    },
                    Screens = e.Screens.Select(s => new ScreenToCinemaDetails
                    {
                        Id = s.Id, 
                        ScreenName = s.ScreenName,
                        Type = s.Type,
                        Status = s.Status,
                        SeatCount = s.Seats.Count,
                    }).ToList()
                });
                
        }
    }
}
