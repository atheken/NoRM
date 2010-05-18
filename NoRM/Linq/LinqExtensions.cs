using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm.BSON;
using Norm.Protocol.Messages;
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
        public static T GetConstantValue<T>(this Expression exp)
        {
            T result = default(T);
            if (exp is ConstantExpression)
            {
                var c = (ConstantExpression) exp;

                result = (T)c.Value;
            }

            return result;
        }

        /// <summary>
        /// Asks Mongo for an explain plan for a linq query.
        /// </summary>
        /// <typeparam name="T">Type to explain</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>Query explain plan</returns>
        public static ExplainResponse Explain<T>(this IQueryable<T> expression)
        {
            var translator = new MongoQueryTranslator();
            var translationResults = translator.Translate(expression.Expression, false);

            if (expression is MongoQuery<T>)
            {
                return (expression as MongoQuery<T>).Explain(translationResults.Where);
            }

            return null;
        }

        /// <summary>
        /// Adds a query hint.
        /// </summary>
        /// <typeparam name="T">Document type</typeparam>
        /// <param name="find">The type of document being enumerated.</param>
        /// <param name="hint">The query hint expression.</param>
        /// <param name="direction">Ascending or descending.</param>
        /// <returns></returns>
        public static IEnumerable<T> Hint<T>(this IEnumerable<T> find, Expression<Func<T, object>> hint, IndexOption direction)
        {
            var translator = new MongoQueryTranslator();
            var index = translator.Translate(hint);

            var proxy = (MongoQueryExecutor<T, Expando>)find;
            proxy.AddHint(index.Query, direction);
            return find;
        }

        /// <summary>
        /// Escapes the double quotes.
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>The escaped string.</returns>
        public static string EscapeDoubleQuotes(this string str)
        {
            return str.Replace("\"", "\\\"");
        }
    }
}
