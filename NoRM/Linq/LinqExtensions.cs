using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm.Responses;

namespace Norm.Linq
{
    /// <summary>
    /// Linq extensions.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Gets the constant value.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns>The get constant value.</returns>
        public static object GetConstantValue(this Expression exp)
        {
            object result = null;
            if (exp is ConstantExpression)
            {
                var c = (ConstantExpression) exp;
                result = c.Value;
            }

            return result;
        }

        /// <summary>
        /// Asks Mongo for an explain plan for a query
        /// </summary>
        /// <typeparam name="T">Type to explain</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>Query explain plan</returns>
        public static ExplainResponse Explain<T>(this IQueryable<T> expression)
        {
            var query = new MongoQueryTranslator().Translate(expression.Expression);

            if (expression is MongoQuery<T>)
            {
                return (expression as MongoQuery<T>).Explain(query);
            }

            return null;
        }

        //public static ExplainResponse Explain<T>(this IEnumerable<T> expression, MongoQueryProvider queryProvider)
        //{
        //    var x = expression.AsQueryable().Expression.GetConstantValue();

        //    var query = new MongoQuery<T>(queryProvider, expression.AsQueryable().Expression);
        //    return query.Explain();
        //}

    }
}