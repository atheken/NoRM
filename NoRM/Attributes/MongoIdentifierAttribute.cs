using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Attributes
{
    /// <summary>
    /// Indicates that the attributed property should be used as the "_id" field within Mongo.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MongoIdentifierAttribute : Attribute
    {
    }
}
