using Norm.Configuration;
using Norm.BSON;
namespace Norm.Responses
{
    /// <summary>TODO::Description.</summary>
    public class CurrentOperationContainer
    {
        static CurrentOperationContainer()
        {
            MongoConfiguration.Initialize(c => c.For<CurrentOperationContainer>(a =>
                                                   {
                                                       a.ForProperty(op => op.Responses).UseAlias("inprog");
                                                   })
                );
        }

        /// <summary>TODO::Description.</summary>
        public CurrentOperationResponse[] Responses{ get; private set;}
    }

    /// <summary>
    /// The current operation response.
    /// </summary>
    public class CurrentOperationResponse : IFlyweight
    {
        static CurrentOperationResponse()
        {
            MongoConfiguration.Initialize(c => c.For<CurrentOperationResponse>(a =>
                                                   {
                                                       a.ForProperty(op => op.OperationId).UseAlias("opid");
                                                       a.ForProperty(op => op.Operation).UseAlias("op");
                                                       a.ForProperty(op => op.Namespace).UseAlias("ns");
                                                       a.ForProperty(op => op.SecondsRunning).UseAlias("secs_running");
                                                   })
                );
        }

        /// <summary>TODO::Description.</summary>
        public int? OperationId { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Operation { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Namespace { get; set; }

        /// <summary>TODO::Description.</summary>
        public string LockType { get; set; }

        /// <summary>TODO::Description.</summary>
        public bool WaitingForLock { get; set; }

        /// <summary>TODO::Description.</summary>
        public bool Active { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Client { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Query { get; set; }

        /// <summary>TODO::Description.</summary>
        public double? InLock { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? SecondsRunning{ get; set;}

        /// <summary>TODO::Description.</summary>
        public string Desc{ get; set;}
    }
}