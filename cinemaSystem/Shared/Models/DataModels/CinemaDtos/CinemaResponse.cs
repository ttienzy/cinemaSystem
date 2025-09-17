using Domain.Entities.CinemaAggreagte.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.CinemaDtos
{
    public class CinemaResponse
    {
        public Guid Id { get; set; }
        public string CinemaName { get; set; }
        public string Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Image { get; set; }
        public CinemaStatus Status { get; set; }
        public List<ScreenResponse> Screens { get; set; } = new();
    }
}
