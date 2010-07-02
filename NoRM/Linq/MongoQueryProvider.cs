using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Norm.BSON;
using Norm.Collections;
using System.Collections;
using Norm.Configuration;

namespace Norm.Linq
{
    /// <summary>
    /// The mongo query provider.
    /// </summary>
    internal class MongoQueryProvider : IQueryProvider, IMongoQueryResults
    {
        private QueryTranslationResults _results;

        internal static MongoQueryProvider Create(IMongoDatabase db, String collectionName)
        {
            return new MongoQueryProvider() { DB = db, CollectionName = collectionName };
        }

        /// <summary>
        /// Gets the DB.
        /// </summary>
        public IMongoDatabase DB
        {
            get;
            private set;
        }

        /// <summary>
        /// The i query provider. create query.
        /// </summary>
        /// <typeparam retval="S">Type of query to create</typeparam>
        /// <param retval="expression">The expression.</param>
        /// <returns></returns>
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            var query = new MongoQuery<S>(this, expression);
            return query;
        }

        public String CollectionName { get; set; }

        /// <summary>
        /// The i query provider. create query.
        /// </summary>
        /// <param retval="expression">The expression.</param>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable"/> that can evaluate the query represented by the specified expression tree.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var elementType = LinqTypeHelper.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(MongoQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// The i query provider. execute.
        /// </summary>
        /// <typeparam retval="S">Type to execute</typeparam>
        /// <param retval="expression">The expression.</param>
        /// <returns>Resulting object</returns>
        S IQueryProvider.Execute<S>(Expression expression)
        {
            object result = ExecuteQuery<S>(expression);
            return (S)Convert.ChangeType(result, typeof(S));
        }

        /// <summary>
        /// The i query provider. execute.
        /// </summary>
        /// <param retval="expression">The expression.</param>
        /// <returns>The i query provider. execute.</returns>
        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        /// <summary>
        /// Executes the Linq expression
        /// </summary>
        /// <param retval="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The execute.</returns>
        public object ExecuteQuery<T>(Expression expression)
        {
            expression = PartialEvaluator.Eval(expression, this.CanBeEvaluatedLocally);

            var translator = new MongoQueryTranslator();
            translator.CollectionName = this.CollectionName;
            var results = translator.Translate(expression);
            _results = results;
            var executor = new MongoQueryExecutor(this.DB, results);

            object retval = null;
            if (results.Select != null)
            {
                MethodInfo mi = executor.GetType().GetMethod("Execute");
                var method = mi.MakeGenericMethod(results.OriginalSelectType);
                retval = method.Invoke(executor, new object[]{});
            }
            else
            {
                retval = executor.Execute<T>();
            }
            return retval;
        }

        /// <summary>
        /// Executes the Linq expression
        /// </summary>
        /// <param retval="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The execute.</returns>
        public object Execute(Expression expression)
        {
            var elementType = LinqTypeHelper.GetElementType(expression.Type);
            try
            {
                return typeof(MongoQueryProvider)
                    .GetMethod("ExecuteQuery")
                    .MakeGenericMethod(elementType)
                    .Invoke(this, new object[] { expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        private bool CanBeEvaluatedLocally(Expression expression)
        {
            // any operation on a query can't be done locally
            ConstantExpression cex = expression as ConstantExpression;
            if (cex != null)
            {
                IQueryable query = cex.Value as IQueryable;
                if (query != null && query.Provider == this)
                    return false;
            }
            MethodCallExpression mc = expression as MethodCallExpression;
            if (mc != null &&
                (mc.Method.DeclaringType == typeof(Enumerable) ||
                 mc.Method.DeclaringType == typeof(Queryable)))
            {
                return false;
            }
            if (expression.NodeType == ExpressionType.Convert &&
                expression.Type == typeof(object))
                return true;
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }

        QueryTranslationResults IMongoQueryResults.TranslationResults
        {
            get { return _results; }
        }

    }
}