using System.Collections.Generic;

namespace Norm.Responses
{
    /// <summary>
    /// The distinct values response.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    internal class DistinctValuesResponse<T> where T : class, new()
    {
        public List<T> Values { get; set; }
        public double? OK { get; set; }
    }
}