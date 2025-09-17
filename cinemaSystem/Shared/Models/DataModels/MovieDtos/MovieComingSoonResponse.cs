using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieComingSoonResponse
    {
        public Guid MovieId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PostUrl { get; set; }
        public List<string> Genres { get; set; }
        public int Duration { get; set; }
        public string Trailer {  get; set; }
    }
}
