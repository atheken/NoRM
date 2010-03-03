namespace NoRM.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class MongoIgnoreIfNullAttribute : Attribute{}
}