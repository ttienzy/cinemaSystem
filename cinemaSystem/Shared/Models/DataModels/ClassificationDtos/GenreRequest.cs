using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.ClassificationDtos
{
    public class GenreRequest
    {
        public required string GenreName { get; set; }
        public string? Description { get; set; }
    }
}
