using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences
{
    public interface IGenreService
    {
        Task<BaseResponse<IEnumerable<Genre>>> GetAllGenresAsync();
        Task<BaseResponse<Genre>> GetGenreByIdAsync(Guid genreId);
        Task<BaseResponse<Genre>> CreateGenreAsync(GenreRequest request);
        Task<BaseResponse<Genre>> UpdateGenreAsync(Guid genreId, GenreRequest request);
        Task<BaseResponse<object>> DeleteGenreAsync(Guid genreId);
    }
}
