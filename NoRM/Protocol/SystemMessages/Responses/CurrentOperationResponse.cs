using Norm.Configuration;
using Norm.BSON;
using System.Collections.Generic;
using System.Linq;

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
              }));
        }

        /// <summary>TODO::Description.</summary>
        public CurrentOperationResponse[] Responses { get; set; }
    }

    /// <summary>
    /// The current operation response.
    /// </summary>
    public class CurrentOperationResponse : IExpando
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

        private Dictionary<string, object> _properties = new Dictionary<string, object>(0);

        /// <summary>TODO::Description.</summary>
        /// <value>The operation Id</value>
        public int? OperationId { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The operation</value>
        public string Operation { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The namespace.</value>
        public string Namespace { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The lock type.</value>
        public string LockType { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>If it is waiting to be locked.</value>
        public bool WaitingForLock { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>If it is active.</value>
        public bool Active { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The client.</value>
        public string Client { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The query.</value>
        public string Query { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>How long it is/was in a lock.</value>
        public double? InLock { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>How long it is/was running</value>
        public int? SecondsRunning { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The description.</value>
        public string Desc { get; set; }

        /// <summary>
        /// Additional, non-static properties of this message.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExpandoProperty> AllProperties()
        {
            return this._properties.Select(j => new ExpandoProperty(j.Key, j.Value));
        }

        public void Delete(string propertyName)
        {
            this._properties.Remove(propertyName);
        }

        public object this[string propertyName]
        {
            get
            {
                return this._properties[propertyName];
            }
            set
            {
                this._properties[propertyName] = value;
            }
        }

    }
}