using System;

namespace NoRM.Attributes
{
    /// <summary>
    /// Ignores properties if the value is null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MongoIgnoreIfNullAttribute : Attribute
    {
    }
}