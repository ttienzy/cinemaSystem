using Application.Interfaces.Persistences.Repo;
using Application.Specifications.MovieSpec;
using Domain.Entities.MovieAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.DataModels.MovieDtos;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly IRepository<Movie> _movieRepository;
        public DemoController(IRepository<Movie> movieRepository)
        {
            _movieRepository = movieRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddCastCrewToMovie([FromQuery] Guid movieId, [FromBody] MovieCastCrewRequest request)
        {
            var spec = new MovieWithCastCrewSpeccification(movieId);
            var movie = await _movieRepository.FirstOrDefaultAsync(spec);
            var x = 123;
            movie.AddCastCrew(request.PersonName, request.RoleType);
            await _movieRepository.UpdateAsync(movie);
            return Ok();
        }
    }
}
