using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieCertificationResponse
    {
        public required Guid Id { get; set; }
        public required string CertificationBody { get; set; }
        public required string Rating { get; set; }
        public required DateTime IssueDate { get; set; }
    }
}
