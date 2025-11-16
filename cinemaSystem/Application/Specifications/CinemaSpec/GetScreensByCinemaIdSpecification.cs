using Ardalis.Specification;
using Domain.Entities.CinemaAggreagte;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.CinemaSpec
{
    public class GetScreensByCinemaIdSpecification : Specification<Cinema, IEnumerable<ScreenInfoDto>>
    {
        public GetScreensByCinemaIdSpecification(Guid cinemaId)
        {
            Query.AsNoTracking()
                .Where(c => c.Id == cinemaId)
                .Include(c => c.Screens)
                .Select(e => e.Screens.Select(s => new ScreenInfoDto
                {
                    ScreenId = s.Id,
                    ScreenName = s.ScreenName,
                    ScreenType = s.Type,
                    SeatCapacity = s.Seats.Count,
                    IsActive = s.Status == Domain.Entities.CinemaAggreagte.Enum.ScreenStatus.Active
                }));
        }
    }
}
