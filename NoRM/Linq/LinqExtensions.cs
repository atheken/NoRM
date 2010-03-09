using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NoRM.BSON;
using NoRM.Protocol.SystemMessages.Responses;

namespace NoRM.Linq
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Gets the constant value.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        public static object GetConstantValue(this Expression exp)
        {
            object result = null;
            if (exp is ConstantExpression)
            {
                ConstantExpression c = (ConstantExpression)exp;
                result = c.Value;
            }
            return result;
        }

        /// <summary>
        /// Asks Mongo for an explain plan for a query
        /// </summary>
        /// <typeparam name="T"></typeparam>
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
    }
}
