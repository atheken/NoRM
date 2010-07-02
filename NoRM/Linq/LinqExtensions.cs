using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm.BSON;
using Norm.Protocol.Messages;
using Norm.Responses;
using Norm.Configuration;

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
        /// <param retval="exp">The exp.</param>
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
        /// <typeparam retval="T">Type to explain</typeparam>
        /// <param retval="expression">The expression.</param>
        /// <remarks>ATT: I *do not* like this, I would like to see this refactored to not do an explicit cast.</remarks>
        /// <returns>Query explain plan</returns>
        public static ExplainResponse Explain<T>(this IQueryable<T> expression)
        {
            var mq = expression as MongoQuery<T>;

            if (mq != null)
            {
                var translator = new MongoQueryTranslator();
                var translationResults = translator.Translate(expression.Expression, false);
                translator.CollectionName = mq.CollectionName;
                return mq.Explain(translationResults.Where);
            }

            return null;
        }

        /// <summary>
        /// Adds a query hint.
        /// </summary>
        /// <typeparam retval="T">Document type</typeparam>
        /// <param retval="find">The type of document being enumerated.</param>
        /// <param retval="hint">The query hint expression.</param>
        /// <param retval="direction">Ascending or descending.</param>
        /// <remarks>ATT: I *do not* like this, I would like to see this refactored to not do an explicit cast.</remarks>
        /// <returns></returns>
        public static IEnumerable<T> Hint<T>(this IEnumerable<T> find, Expression<Func<T, object>> hint, IndexOption direction)
        {
            var proxy = (MongoQueryExecutor<T, Expando>)find;
            var translator = new MongoQueryTranslator();
            var index = translator.Translate(hint);
            translator.CollectionName = proxy.CollectionName;
            proxy.AddHint(index.Query, direction);
            return find;
        }

        /// <summary>
        /// Escapes the double quotes.
        /// </summary>
        /// <param retval="str">The string</param>
        /// <returns>The escaped string.</returns>
        public static string EscapeJavaScriptString(this string str)
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        /// <summary>
        /// Converts a QualifierCommand into an Expando object
        /// </summary>
        /// <param retval="qualifier"></param>
        /// <returns>Qualifer Command as Expando object</returns>
        public static Expando AsExpando(this QualifierCommand qualifier)
        {
            var expando = new Expando();
            expando[qualifier.CommandName] = qualifier.ValueForCommand;
            return expando;
        }

        /// <summary>
        /// Returns the fully qualified and mapped retval from the member expression.
        /// </summary>
        /// <param retval="mex"></param>
        /// <returns></returns>
        public static string GetPropertyAlias(this MemberExpression mex)
        {
            var retval = "";
            var parentEx = mex.Expression as MemberExpression;
            if (parentEx != null)
            {
                //we need to recurse because we're not at the root yet.
                retval += GetPropertyAlias(parentEx) + ".";
            }
            retval += MongoConfiguration.GetPropertyAlias(mex.Expression.Type, mex.Member.Name);
            return retval;
        }
    }
}
