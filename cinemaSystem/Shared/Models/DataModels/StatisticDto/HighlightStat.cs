using Shared.Models.DataModels.CinemaDtos;
using Shared.Models.DataModels.MovieDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.StatisticDto
{
    public class HighlightStat
    {
        public IEnumerable<CinemaBaseResponse> CinemaBaseResponse { get; set; } 
        public IEnumerable<MovieBaseResponse> MovieBaseResponse { get; set; }
        public StatisticItemResponse StatisticItemResponse { get; set; }

    }
}
