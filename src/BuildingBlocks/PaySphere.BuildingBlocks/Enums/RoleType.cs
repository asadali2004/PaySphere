using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaySphere.BuildingBlocks.Enums
{
    /// <summary>
    /// Numeric role identifiers to seed and reference roles in code and tests.
    /// Prefer role names for runtime checks; these numeric values are useful
    /// for database seeding and test assertions.
    /// </summary>
    public enum RoleType
    {
        Admin = 1,
        User = 2
    }
}
