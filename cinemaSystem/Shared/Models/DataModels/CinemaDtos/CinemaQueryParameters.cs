using Shared.Common.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.CinemaDtos
{
    public class CinemaQueryParameters : PagingQueryParameters
    {
        public string? CinemaName { get; set; }
    }
}
