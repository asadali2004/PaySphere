using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaySphere.BuildingBlocks.Pagination
{
    /// <summary>
    /// Simple pagination request used by endpoints that return lists.
    /// Encapsulates page number and size and enforces a maximum size to protect the server from excessive load.
    /// </summary>
    public class PaginationRequest
    {
        // Maximum allowed page size to prevent overly expensive requests.
        private const int MaxPageSize = 100;

        // Default to the first page (1-based indexing used consistently across the API).
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        // PageSize setter caps the value to MaxPageSize to protect performance.
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
