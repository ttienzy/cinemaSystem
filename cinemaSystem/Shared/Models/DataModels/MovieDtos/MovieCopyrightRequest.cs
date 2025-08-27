using Domain.Entities.MovieAggregate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieCopyrightRequest
    {
        public string? DistributorCompany { get; set; }
        public DateTime LicenseStartDate { get; set; }
        public DateTime LicenseEndDate { get; set; }
        public MovieCopyrightStatus Status { get; set; } // active, expired, pending
    }
}
