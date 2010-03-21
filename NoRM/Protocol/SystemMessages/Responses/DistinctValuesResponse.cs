using System.Collections.Generic;
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The distinct values response.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    internal class DistinctValuesResponse<T> : BaseStatusMessage where T : class, new()
    {
        /// <summary>
        /// Initializes the <see cref="DistinctValuesResponse&lt;T&gt;"/> class.
        /// </summary>
        static DistinctValuesResponse()
        {
            MongoConfiguration.Initialize(c => c.For<DistinctValuesResponse<T>>(a => a.ForProperty(auth => auth.Ok).UseAlias("ok"))
                );
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>The values.</value>
        public List<T> Values { get; set; }
    }
}