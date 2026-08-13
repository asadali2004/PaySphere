using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaySphere.BuildingBlocks.Responses
{
    /// <summary>
    /// Standard API response wrapper used across services to provide a consistent shape
    /// for success/failure, messages, data payload and error details.
    /// Controllers return this type so clients and the API Gateway can rely on a stable contract.
    /// </summary>
    /// <typeparam name="T">Type of the Data payload when the call is successful.</typeparam>
    public class ApiResponse<T>
    {
        // Indicates whether the operation completed successfully.
        public bool Success { get; set; }

        // Human readable message intended for clients or logs.
        public string Message { get; set; } = string.Empty;

        // Optional payload when Success is true. Kept nullable to represent error responses.
        public T? Data { get; set; }

        // Optional list of error codes/messages for failed requests.
        public List<string>? Errors { get; set; }
    }
}
