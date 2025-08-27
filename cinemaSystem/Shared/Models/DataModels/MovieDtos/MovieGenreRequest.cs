using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieGenreRequest
    {
        public required Guid GenreId { get; set; }  
    }
}
