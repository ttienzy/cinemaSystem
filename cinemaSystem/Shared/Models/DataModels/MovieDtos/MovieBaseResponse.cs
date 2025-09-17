using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieBaseResponse
    {
        public Guid MovieId { get; set; }
        public string Title { get; set; }
    }
}
