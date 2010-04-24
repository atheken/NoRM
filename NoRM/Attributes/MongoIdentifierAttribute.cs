using System;

namespace Norm
{
    /// <summary>
    /// Flags a property as a Mongo identifier (_id)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MongoIdentifierAttribute : Attribute
    {
    }
}