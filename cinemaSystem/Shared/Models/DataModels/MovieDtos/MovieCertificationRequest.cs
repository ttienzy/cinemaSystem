using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieCertificationRequest
    {
        public required string CertificationBody { get; set; }
        public required string Rating { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    }
}
