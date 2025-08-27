using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieCopyrightResponse
    {
        public required Guid Id { get; set; }
        public required string DistributorCompany { get; set; }
        public DateTime LicenseStartDate { get; set; }
        public DateTime LicenseEndDate { get; set; }
    }
}
