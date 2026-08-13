using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaySphere.BuildingBlocks.Responses
{
    /// <summary>
    /// Response wrapper for paginated results. Inherits ApiResponse with an enumerable payload.
    /// Pagination metadata (page number/size/total) is included to assist clients in paging UI and API callers.
    /// </summary>
    public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
    {
        // Current 1-based page number returned.
        public int PageNumber { get; set; }

        // Number of items requested per page.
        public int PageSize { get; set; }

        // Total number of items matching the query (used to compute total pages client-side).
        public int TotalRecords { get; set; }

        // Total pages computed from TotalRecords and PageSize.
        public int TotalPages { get; set; }
    }
}
