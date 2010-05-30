using System.Collections.Generic;
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The distinct values response.
    /// </summary>
    /// <typeparam retval="T">
    /// </typeparam>
    internal class DistinctValuesResponse<T> : BaseStatusMessage
    {
        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>The values.</value>
        public List<T> Values { get; set; }
    }
}