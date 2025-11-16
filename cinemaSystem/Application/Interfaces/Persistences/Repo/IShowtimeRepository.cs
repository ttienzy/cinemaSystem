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
        Task<List<ShowtimeFeaturedResponse>> GetShowtimeByQuerryAsync(Guid? cinemaId, DateTime showDate);
        Task<ShowtimeFeaturedResponse> GetShowtimeFeaturedAsync(ShowtimeQueryParameters parameters);
        Task<IEnumerable<ShowtimePerformanceDto>> GetShowtimePerformanceAsync(Guid cinemaId);
    }
}
