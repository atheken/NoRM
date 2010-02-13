using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Mongo.Commands.Qualifiers;
using System.Text.RegularExpressions;

namespace System.Data.Mongo
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
