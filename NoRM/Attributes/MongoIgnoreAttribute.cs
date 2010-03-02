using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Attributes
{
    /// <summary>
    /// Indicates that the BSON serializer should ignore the property on which this attribute is applied.
    /// </summary>
    /// <remarks>
    /// BsonSerializer doesn't actually check for these, YET!
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class MongoIgnoreAttribute : Attribute
    {
        public MongoIgnoreAttribute()
        {
        }
    }
}
