using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Paging
{
    public class PaginationResponse
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int Count { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
