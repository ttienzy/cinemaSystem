using Shared.Models.DataModels.CinemaDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Persistences.Repo
{
    public interface ICinemaRepository
    {
        IQueryable<CinemaResponse> GetAllCinemasAsQueryable();
    }
}
