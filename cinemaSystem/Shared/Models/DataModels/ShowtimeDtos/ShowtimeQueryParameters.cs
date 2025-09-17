using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ShowtimeDtos
{
    public class ShowtimeQueryParameters
    {
        public Guid MovieId { get; set; }
        public required DateTime ShowDate { get; set; }
        public required Guid CinemaId { get; set; }
    }
}
