using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo
{
    /// <summary>
    /// This is a marker interface that allows 
    /// expando properties to gotten from a special cache via extension methods.
    /// </summary>
    public interface IMongoFlyweight
    {
    }

    /// <summary>
    /// These will allow an object that doesn't have a particular property defined to still provide it under special circumstances...
    /// you're better off using a class def. to define properties whenever possible.
    /// </summary>
    public static class MongoFlyweightExtensions
    {
        public static bool? GetBoolean(this IMongoFlyweight obj, String key)
        {
            return null;
        }

        public static int? GetInt(this IMongoFlyweight obj, String key)
        {
            return null;
        }
        
        public static double? GetDouble(this IMongoFlyweight obj, String key)
        {
            return null;
        }

        public static String GetString(this IMongoFlyweight obj, String key)
        {
            return null;
        }
    }
}
