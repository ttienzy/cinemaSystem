using Domain.Entities.CinemaAggreagte.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.CinemaDtos
{
    public class ScreenRequest
    {
        public string ScreenName { get; set; }
        public ScreenType Type { get; set; }
        public ScreenStatus Status { get; set; }
    }
}
