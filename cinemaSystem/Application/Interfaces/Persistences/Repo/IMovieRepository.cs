using Domain.Entities.MovieAggregate;
using Domain.Entities.MovieAggregate.Enum;
using Shared.Models.DataModels.MovieDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences.Repo
{
    public interface IMovieRepository
    {
        IQueryable<MovieResponse> GetMovies(string? title, MovieStatus movieStatus);
    }
}
