using System;

namespace Norm.Attributes
{
    /// <summary>
    /// Indicates that the BSON serializer should ignore the property on which this attribute is applied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MongoIgnoreAttribute : Attribute
    {
    }
}