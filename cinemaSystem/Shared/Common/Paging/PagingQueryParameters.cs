using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Paging
{
    public class PagingQueryParameters
    {
        const int maxPageSize = 12;
        private int _pageSize = 12;
        public int PageIndex { get; set; } = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (value > maxPageSize)
                    _pageSize = value;
            }
        }
    }
}
