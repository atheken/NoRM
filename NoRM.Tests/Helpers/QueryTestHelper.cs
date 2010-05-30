using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Linq;

namespace Norm.Tests.Helpers
{
    public static class QueryTestHelper
    {
        /// <summary>
        /// Provides a way to pull the results of the query construction.. This is an implementation detail, 
        /// but used to verify that our queries are appropriately optimized.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static QueryTranslationResults QueryStructure<T>(this IQueryable<T> queryable)
        {
             return ((IMongoQueryResults)queryable.Provider).TranslationResults;;
        }

    }
}
