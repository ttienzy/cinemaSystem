using Application.Common.Interfaces.Persistence;
using MediatR;
using Shared.Models.DataModels.SharedDtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Shared.Genres.Queries.GetAll
{
    public record GetAllGenresQuery(bool ActiveOnly = false) : IRequest<List<GenreDto>>;

    public class GetAllGenresHandler(IGenreRepository genreRepo) : IRequestHandler<GetAllGenresQuery, List<GenreDto>>
    {
        public async Task<List<GenreDto>> Handle(GetAllGenresQuery request, CancellationToken ct)
        {
            var genres = await genreRepo.GetAllAsync(ct);

            if (request.ActiveOnly)
            {
                genres = genres.Where(g => g.IsActive).ToList();
            }

            return genres.Select(g => new GenreDto
            {
                Id = g.Id,
                GenreName = g.GenreName,
                Description = g.Description,
                IsActive = g.IsActive
            }).ToList();
        }
    }
}
