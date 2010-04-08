using Norm.Configuration;
using Norm.BSON;
namespace Norm.Responses
{
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
        
        public int? OperationId { get; set; }
        public string Operation { get; set; }
        public string Namespace { get; set; }
        public string LockType { get; set; }
        public bool WaitingForLock { get; set; }
        public bool Active { get; set; }
        public string Client { get; set; }
        public string Query { get; set; }
        public double? InLock { get; set; }        
        public int? SecondsRunning{ get; set;}
        public string Desc{ get; set;}
    }
}