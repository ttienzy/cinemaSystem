using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.CinemaDtos
{
    public class CinemaBaseResponse
    {
        public Guid CinemaId { get; set; }
        public string CinemaName { get; set; }
        public string Address { get; set; }
    }

}
