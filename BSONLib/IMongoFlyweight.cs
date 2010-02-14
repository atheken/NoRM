using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSONLib
{
    /// <summary>
    /// This is a marker interface that allows 
    /// expando properties to gotten from a special cache via extension methods.
    /// </summary>
    public interface IMongoFlyweight
    {
    }

    ///// <summary>
    ///// These will allow an object that doesn't have a particular property defined 
    ///// to still provide it under special circumstances...
    ///// you're better off using a class def. to define properties whenever possible.
    ///// </summary>
    //public static class MongoFlyweightExtensions
    //{
    //    public static T Get<T>(this IMongoFlyweight obj, String propertyName) where T : class
    //    {
    //        return ExpandoProps.Get<T>(obj, propertyName);
    //    }
    //    public static void Set<T>(this IMongoFlyweight obj, String propertyName, T value)
    //    {
    //        ExpandoProps.Set(obj, propertyName, value);
    //    }

    //    public static void DeleteProperty(this IMongoFlyweight obj, String propertyName)
    //    {
    //        ExpandoProps.DeleteProperty(obj, propertyName);
    //    }
    //}
}
