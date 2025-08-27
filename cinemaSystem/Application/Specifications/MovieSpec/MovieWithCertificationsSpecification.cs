using Ardalis.Specification;
using Domain.Entities.MovieAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Specifications.MovieSpec
{
    public class MovieWithCertificationsSpecification : Specification<Movie>
    {
        public MovieWithCertificationsSpecification(Guid movieId)
        {
            Query.Where(m => m.Id == movieId)
                .Include(m => m.Certifications);
        }
        public MovieWithCertificationsSpecification(Guid movieId, Guid certId)
        {
            Query.Where(m => m.Id == movieId && m.Certifications.Any(c => c.Id == certId))
                .Include(m => m.Certifications);
        }
        public MovieWithCertificationsSpecification(Guid movieId, List<Guid> certificationIds)
        {
            Query.Where(m => m.Id == movieId && m.Certifications.Any(c => certificationIds.Contains(c.Id)))
                .Include(m => m.Certifications);
        }
    }
}
