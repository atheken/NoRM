using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Configuration;

namespace NoRM.Protocol.SystemMessages
{
    /// <summary>
    /// Provides a message that sends a system Query to the server.
    /// </summary>
    internal class ExplainRequest : ISystemQuery
    {
        static ExplainRequest()
        {
            MongoConfiguration.Initialize(c =>
                c.For<ExplainRequest>(a =>
                    {
                        a.ForProperty(ex => ex.Explain).UseAlias("$explain");
                        a.ForProperty(ex => ex.Query).UseAlias("query");
                    })
                );
        }

        /// <summary>
        /// Indicates that the explain query should be
        /// </summary>
        public bool Explain
        {
            get
            {
                return true;
            }

        }
        
        /// <summary>
        /// The query to be explained.
        /// </summary>
        public object Query
        {
            get;
            set;
        }
    }
}
