using Domain.Entities.MovieAggregate.Enum;
using Shared.Common.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.DataModels.MovieDtos
{
    public class MovieQueryParameters : PagingQueryParameters
    {
        public string? Title { get; set; }
        public MovieStatus Status { get; set; } = MovieStatus.Showing;
    }
}
