using Application.Interfaces.Persistences;
using Application.Interfaces.Persistences.Repo;
using Domain.Entities.SharedAggregates;
using Shared.Common.Base;
using Shared.Models.DataModels.ClassificationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Services
{
    public class GenreService : IGenreService
    {
        private readonly IRepository<Genre> _genreRepository;
        public GenreService(IRepository<Genre> genreRepository)
        {
            _genreRepository = genreRepository ?? throw new ArgumentNullException(nameof(genreRepository));
        }
        public async Task<BaseResponse<Genre>> CreateGenreAsync(GenreRequest request)
        {
            try
            {
                var genre = new Genre(request.GenreName, request.Description);
                await _genreRepository.AddAsync(genre);
                return BaseResponse<Genre>.Success(genre);
            }
            catch (Exception ex)
            {
                return BaseResponse<Genre>.Failure(Error.InternalServerError($"An error occurred while creating the genre: {ex.Message}"));
            }
        }

        public async Task<BaseResponse<object>> DeleteGenreAsync(Guid genreId)
        {
            try
            {
                var genre = await _genreRepository.GetByIdAsync(genreId);
                if (genre == null)
                {
                    return BaseResponse<object>.Failure(Error.NotFound($"Genre with ID {genreId} not found."));
                }
                await _genreRepository.DeleteAsync(genre);
                return BaseResponse<object>.Success();
            }
            catch (Exception ex)
            {
                return BaseResponse<object>.Failure(Error.InternalServerError($"An error occurred while creating the genre: {ex.Message}"));
            }
        }

        public async Task<BaseResponse<IEnumerable<Genre>>> GetAllGenresAsync()
        {
            try
            {
                var genres = await _genreRepository.ListAsync();
                return BaseResponse<IEnumerable<Genre>>.Success(genres);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<Genre>>.Failure(Error.InternalServerError($"An error occurred while creating the genre: {ex.Message}"));
            }
        }

        public async Task<BaseResponse<Genre>> GetGenreByIdAsync(Guid genreId)
        {
            try
            {
                var genre = await _genreRepository.GetByIdAsync(genreId);
                if (genre == null)
                {
                    return BaseResponse<Genre>.Failure(Error.NotFound($"Genre with ID {genreId} not found."));
                }
                return BaseResponse<Genre>.Success(genre);
            }
            catch (Exception ex)
            {
                return BaseResponse<Genre>.Failure(Error.InternalServerError($"An error occurred while creating the genre: {ex.Message}"));
            }
        }

        public async Task<BaseResponse<Genre>> UpdateGenreAsync(Guid genreId, GenreRequest request)
        {
            try
            {
                var genreEntity = await _genreRepository.GetByIdAsync(genreId);
                if (genreEntity == null)
                {
                    return BaseResponse<Genre>.Failure(Error.NotFound($"Genre with ID {genreId} not found."));
                }
                genreEntity.UpdateGenre(request.GenreName, request.Description);
                await _genreRepository.UpdateAsync(genreEntity);
                return BaseResponse<Genre>.Success(genreEntity);
            }
            catch (Exception ex)
            {
                return BaseResponse<Genre>.Failure(Error.InternalServerError($"An error occurred while creating the genre: {ex.Message}"));
            }
        }
    }
}
