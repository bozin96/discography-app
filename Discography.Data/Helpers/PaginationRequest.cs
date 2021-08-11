using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discography.Data.Helpers
{
    public class PaginationRequest
    {
        const int maxPageSize = 20;
        public string SearchQuery { get; set; }
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public string OrderBy { get; set; } = "Id";
        public string Fields { get; set; }
    }
}
