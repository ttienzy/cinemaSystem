using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieGenreResponse
    {
        public required Guid Id { get; set; }
        public required string GenreName { get; set; }
    }
}
