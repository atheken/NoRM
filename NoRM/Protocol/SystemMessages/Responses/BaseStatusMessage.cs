using Norm.Configuration;
using Norm.BSON;
using System.Collections.Generic;
using System.Linq;

namespace Norm.Responses
{
    /// <summary>
    /// Represents a message with an Ok status
    /// </summary>
    public class BaseStatusMessage : IExpando
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>(0);

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
