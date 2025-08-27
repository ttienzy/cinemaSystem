using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieCastCrewResponse
    {
        public required Guid Id { get; set; }
        public required string PersonName {get; set; }
        public required string RoleType { get; set; }
    }
}
