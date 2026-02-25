using Domain.Entities.CinemaAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.CinemaDtos
{
    public class ScreenResponse
    {
        public Guid Id { get; set; }
        public string ScreenName { get; set; }
        public ScreenType Type { get; set; }
        public ScreenStatus Status { get; set; }
    }
}
