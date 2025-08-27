using Domain.Entities.ShowtimeAggregate;
using Shared.Models.DataModels.ShowtimeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences.Repo
{
    public interface IShowtimeRepository
    {
        Task<List<ShowtimeDetailsResponse>> GetShowtimeByQuerryAsync(Guid? cinemaId, Guid? movieId, DateTime showDate);
        //Task<ShowtimeSeatingPlanResponse> GetShowtimeSeatingPlanAsync(Guid showtimeId);
    }
}
