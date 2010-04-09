//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Norm.BSON;
//using Norm.Configuration;

//namespace Norm.Protocol.SystemMessages
//{
//    /// <summary>
//    /// Provides a message that sends a system Query to the server.
//    /// </summary>
//    internal class ExplainRequest : ISystemQuery
//    {
//        static ExplainRequest()
//        {
//            MongoConfiguration.Initialize(c => c.For<ExplainRequest>(a =>
//                    {
//                        //a.ForProperty(ex => ex.Explain).UseAlias("$explain");
//                        a.ForProperty(ex => ex.Query).UseAlias("query");
//                    })
//                );
//        }

//        public ExplainRequest(object query)
//        {
//            Query = new Flyweight();
//            Query["$query"] = query;
//            Query["$explain"] = true;
//        }

//        ///// <summary>
//        ///// Indicates that the explain query should be
//        ///// </summary>
//        //public bool Explain
//        //{
//        //    get
//        //    {
//        //        return true;
//        //    }

//        //}
        
//        /// <summary>
//        /// The query to be explained.
//        /// </summary>
//        public Flyweight Query
//        {
//            get;
//            private set;
//        }
//    }
//}
