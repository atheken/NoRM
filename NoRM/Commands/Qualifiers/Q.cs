using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoSharp.Commands.Qualifiers;
using System.Text.RegularExpressions;

namespace MongoSharp
{
    /// <summary>
    /// Qualifier operations.
    /// Provides a way to specify some of the "special" qualifiers that can be used for querying.
    /// </summary>
    /// <remarks>
    /// This should remain in the System.Data.Mongo namespace so that it's available 
    /// automatically when someone is using a MongoCollection.
    /// </remarks>
    public class Q
    {
        /// <summary>
        /// Builds a $lt qualifier for the search.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static LessThanQualifier LessThan(double value)
        {
            return new LessThanQualifier(value);
        }

        /// <summary>
        /// Builds a $lte qualifier for the search.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static LessOrEqualQualifier LessOrEqual(double value)
        {
            return new LessOrEqualQualifier(value);
        }

        /// <summary>
        /// Builds a $gte qualifier for the search.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static GreaterOrEqualQualifier GreaterOrEqual(double value)
        {
            return new GreaterOrEqualQualifier(value);
        }

        /// <summary>
        /// Builds a $gt qualifier for the search.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static GreaterThanQualifier GreaterThan(double value)
        {
            return new GreaterThanQualifier(value);
        }

        /// <summary>
        /// Builds an $all statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="all"></param>
        /// <returns></returns>
        public static AllQualifier<T> All<T>(params T[] all)
        {
            return new AllQualifier<T>(all);
        }

        /// <summary>
        /// Builds an $in qualifier statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inSet"></param>
        /// <returns></returns>
        public static InQualifier<T> In<T>(params T[] inSet)
        {
            return new InQualifier<T>(inSet);
        }

        /// <summary>
        /// Builds a $ne qualifier against the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="test"></param>
        /// <returns></returns>
        public static NotEqualQualifier NotEqual<T>(T test)
        {
            return new NotEqualQualifier(test);
        }

        /// <summary>
        /// Passes the value straight back to you, new { Property = "value"} will 
        /// work just fine as a qualifier. Here for the sake of consistency.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="test"></param>
        /// <returns></returns>
        public static T Equals<T>(T test)
        {
            return test;
        }

        /// <summary>
        /// Builds a $size qualifier.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static SizeQualifier Size(double size)
        {
            return new SizeQualifier(size);
        }

        /// <summary>
        /// Builds an $nin qualifier statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inSet"></param>
        /// <returns></returns>
        public static NotInQualifier<T> NotIn<T>(params T[] inSet)
        {
            return new NotInQualifier<T>(inSet);
        }


        /// <summary>
        /// Builds an $exists qualifier for the search.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ExistsQuallifier Exists(bool value)
        {
            return new ExistsQuallifier(value);
        }
    }
}
