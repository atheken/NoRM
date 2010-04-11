using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Configuration;
using Norm.Protocol.SystemMessages;

namespace Norm.Protocol
{
    /// <summary>TODO::Description.</summary>
    public class ExplainRequest<T>  : ISystemQuery
    {
        static ExplainRequest()
        {
            MongoConfiguration.Initialize(cfg=>cfg.For<ExplainRequest<T>>(y=>{
                y.ForProperty(c=>c.Explain).UseAlias("$explain");
                y.ForProperty(c => c.Query).UseAlias("query"); 
            }));
        }

        /// <summary>TODO::Description.</summary>
        /// <param name="query">The query.</param>
        public ExplainRequest(T query)
        {
            this.Query = query;
        }

        /// <summary>
        /// Tells the server to explain the query.
        /// </summary>
        /// <remarks>
        /// THIS SHOULD ALWAYS BE DEFINED BEFORE QUERY!
        /// </remarks>
        private bool Explain { get { return true; } }

        /// <summary>
        /// The query that should be used.
        /// </summary>
        public T Query { get; protected set; }
    }
}
