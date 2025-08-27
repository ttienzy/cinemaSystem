using Domain.Entities.CinemaAggreagte.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.CinemaDtos
{
    public class CinemaRequest
    {
        public string CinemaName { get; set; }
        public string Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string ManagerName { get; set; }
        public CinemaStatus Status { get; set; }
    }
}
