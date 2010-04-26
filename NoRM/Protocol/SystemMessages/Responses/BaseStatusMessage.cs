using Norm.Configuration;

namespace Norm.Responses
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a message with an Ok status
    /// </summary>
    public class BaseStatusMessage : IExpando
    {
        static BaseStatusMessage()
        {
            MongoConfiguration.Initialize(c => c.For<BaseStatusMessage>(a =>
                       {
                           a.ForProperty(auth => auth.Ok).UseAlias("ok");
                       })
                );
        }

        /// <summary>
        /// The operation status
        /// </summary>
        /// <value>The Ok property gets the Ok data member.</value>
        public double? Ok { get; set; }

        private IDictionary<string, object> _expando;
        public IDictionary<string, object> Expando
        {
            get
            {
                if (_expando == null) { _expando = new Dictionary<string, object>();}
                return _expando;
            }
        }
    }
}
