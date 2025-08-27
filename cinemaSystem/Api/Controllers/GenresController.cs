using Application.Interfaces.Persistences;
using Domain.Entities.SharedAggregates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _genreService;
        public GenresController(IGenreService genreService)
        {
            _genreService = genreService ?? throw new ArgumentNullException(nameof(genreService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGenresAsync()
        {
            var response = await _genreService.GetAllGenresAsync();
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorReponse<IEnumerable<Genre>>.WithError(response);
        }
        [HttpGet("{genreId}")]
        public async Task<IActionResult> GetGenreByIdAsync(Guid genreId)
        {
            var response = await _genreService.GetGenreByIdAsync(genreId);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorReponse<Genre>.WithError(response);
        }
        [HttpPost]
        public async Task<IActionResult> CreateGenreAsync([FromBody] GenreRequest request)
        {
            if (request == null)
            {
                return BadRequest("Genre request cannot be null.");
            }
            var response = await _genreService.CreateGenreAsync(request);
            if (response.IsSuccess)
            {
                return CreatedAtAction(nameof(GetGenreByIdAsync), new { genreId = response.Value.Id }, response.Value);
            }
            return ErrorReponse<Genre>.WithError(response);
        }
        [HttpPut("{genreId}")]
        public async Task<IActionResult> UpdateGenreAsync(Guid genreId, [FromBody] GenreRequest request)
        {
            if (request == null)
            {
                return BadRequest("Genre request cannot be null.");
            }
            var response = await _genreService.UpdateGenreAsync(genreId, request);
            if (response.IsSuccess)
            {
                return Ok(response.Value);
            }
            return ErrorReponse<Genre>.WithError(response);
        }
        [HttpDelete("{genreId}")]
        public async Task<IActionResult> DeleteGenreAsync(Guid genreId)
        {
            var response = await _genreService.DeleteGenreAsync(genreId);
            if (response.IsSuccess)
            {
                return NoContent();
            }
            return ErrorReponse<object>.WithError(response);
        }
    }
}
