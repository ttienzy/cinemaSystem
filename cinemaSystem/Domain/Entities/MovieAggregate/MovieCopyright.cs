using Domain.Common;
using Domain.Entities.MovieAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MovieAggregate
{
    public class MovieCopyright : BaseEntity
    {
        public Guid MovieId { get; private set; }
        public string? DistributorCompany { get; private set; }
        public DateTime LicenseStartDate { get; private set; }
        public DateTime LicenseEndDate { get; private set; }
        public MovieCopyrightStatus? Status { get; private set; } // active, expired, pending

        public MovieCopyright()
        {
            Id = Guid.NewGuid();
        }
        public MovieCopyright(string distributorCompany, DateTime licenseStartDate, DateTime licenseEndDate, MovieCopyrightStatus status)
        {
            Id = Guid.NewGuid();
            DistributorCompany = distributorCompany;
            LicenseStartDate = licenseStartDate;
            LicenseEndDate = licenseEndDate;
            Status = status;
        }
        public void UpdateDetail(string distributorCompany, DateTime licenseStartDate, DateTime licenseEndDate, MovieCopyrightStatus status)
        {
            DistributorCompany = distributorCompany;
            LicenseStartDate = licenseStartDate;
            LicenseEndDate = licenseEndDate;
            Status = status;
        }
    }
}
