using System;

namespace Norm.Attributes
{
    /// <summary>
    /// Ignores properties on updates, but not on inserts, i.e. this is write-once value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MongoImmutableAttribute : Attribute
    {
    }
}
