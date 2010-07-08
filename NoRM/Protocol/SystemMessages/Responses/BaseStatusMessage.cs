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
        
        /// <summary>
        /// This is the raw value returned from the response. 
        /// It is required for serializer support, use "WasSuccessful" if you need a boolean value.
        /// </summary>
        /// <remarks>This maps to the "OK" value of the response which can be a decimal (pre-1.5.2) or boolean (1.5.2+).</remarks>
        public bool WasSuccessful
        {
            get
            {
                return this._properties.ContainsKey("ok") && (this["ok"].Equals(true) || this["ok"].Equals(1d));
            }
        }

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
