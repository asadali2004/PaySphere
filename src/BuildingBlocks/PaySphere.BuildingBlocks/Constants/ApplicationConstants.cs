using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaySphere.BuildingBlocks.Constants
{
    /// <summary>
    /// Shared application constants used across services to keep magic values in one place.
    /// </summary>
    public static class ApplicationConstants
    {
        public const string AdminRole = "Admin";

        public const string UserRole = "User";

        // Default and maximum page sizes used by pagination helpers and controllers.
        public const int DefaultPageSize = 10;

        public const int MaximumPageSize = 100;
    }
}
