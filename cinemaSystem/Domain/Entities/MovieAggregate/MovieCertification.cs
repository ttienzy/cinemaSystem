using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MovieAggregate
{
    public class MovieCertification : BaseEntity
    {
        public Guid MovieId { get; private set; } 
        public string? CertificationBody { get; private set; } // e.g., Vietnam Cinema Department
        public string? Rating { get; private set; } // e.g., P, T13, T16, T18
        public DateTime IssueDate { get; private set; }

        public MovieCertification()
        {
        }
        public MovieCertification(Guid movieId,string certificationBody, string rating, DateTime issueDate)
        {
            MovieId = movieId;
            CertificationBody = certificationBody;
            Rating = rating;
            IssueDate = issueDate;
        }
        public void UpdateDetail( string certificationBody, string rating, DateTime issueDate)
        {
            CertificationBody = certificationBody;
            Rating = rating;
            IssueDate = issueDate;
        }
    }
}
