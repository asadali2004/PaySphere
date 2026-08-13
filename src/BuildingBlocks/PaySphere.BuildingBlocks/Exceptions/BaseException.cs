using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaySphere.BuildingBlocks.Exceptions
{
    /// <summary>
    /// Lightweight domain exception used to represent business-rule failures that should be
    /// translated to specific HTTP responses by controllers or middleware.
    /// Keeping a dedicated exception type avoids coupling to framework exception classes.
    /// </summary>
    public class BaseException : Exception
    {
        public BaseException(string message)
            : base(message)
        {
        }
    }
}
