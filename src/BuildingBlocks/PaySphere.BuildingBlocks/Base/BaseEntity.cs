using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaySphere.BuildingBlocks.Base
{
    /// <summary>
    /// Base class for all persisted entities in the system.
    /// Keeps only common persistence fields so domain classes remain focused on business data.
    /// </summary>
    public abstract class BaseEntity
    {
        // Primary key used by EF Core.
        public int Id { get; set; }

        // UTC timestamp for auditing when the record was created.
        public DateTime CreatedAt { get; set; }

        // Nullable UTC timestamp updated when the record is modified.
        // Left nullable to represent records that have never been updated after creation.
        public DateTime? UpdatedAt { get; set; }
    }
}
