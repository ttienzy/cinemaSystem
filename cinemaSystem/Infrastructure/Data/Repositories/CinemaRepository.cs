using Application.Interfaces.Persistences.Repo;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DataModels.CinemaDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class CinemaRepository : ICinemaRepository
    {
        private readonly BookingContext _context;
        public CinemaRepository(BookingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Booking context cannot be null");
        }
        public IQueryable<CinemaResponse> GetAllCinemasAsQueryable()
        {
            return _context.Cinemas
                .Include(c => c.Screens)
                .Select(cinema => new CinemaResponse
                {
                    Id = cinema.Id,
                    Address = cinema.Address,
                    Phone = cinema.Phone,
                    Email = cinema.Email,
                    Website = cinema.Website,
                    CinemaName = cinema.CinemaName,
                    Screens = cinema.Screens.Select(screen => new ScreenResponse
                    {
                        Id = screen.Id,
                        ScreenName = screen.ScreenName,
                        Type = screen.Type,
                        Status = screen.Status
                    }).ToList()
                });
        }
    }
}
