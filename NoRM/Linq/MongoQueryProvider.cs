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
    public class MongoQueryProvider : IQueryProvider, IDisposable, IMongoQueryResults
    {
        private readonly Mongo _server;
        private QueryTranslationResults _results;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryProvider"/> class.
        /// </summary>
        /// <param name="server">
        /// The server.
        /// </param>
        public MongoQueryProvider(Mongo server)
        {
            _server = server;
        }

        public static MongoQueryProvider Create(String connectionString)
        {
            return new MongoQueryProvider(Mongo.Create(connectionString));
        }

        /// <summary>
        /// Gets the DB.
        /// </summary>
        public MongoDatabase DB
        {
            get { return _server.Database; }
        }

        /// <summary>
        /// Gets the server.
        /// </summary>
        public Mongo Server
        {
            get { return _server; }
        }

        /// <summary>
        /// The i query provider. create query.
        /// </summary>
        /// <typeparam name="S">Type of query to create</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            var query = new MongoQuery<S>(this, expression);
            return query;
        }

        /// <summary>
        /// The i query provider. create query.
        /// </summary>
        /// <param name="expression">The expression.</param>
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
        /// <typeparam name="S">Type to execute</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>Resulting object</returns>
        S IQueryProvider.Execute<S>(Expression expression)
        {
            object result = ExecuteQuery<S>(expression);
            return (S)Convert.ChangeType(result, typeof(S));
        }

        /// <summary>
        /// The i query provider. execute.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The i query provider. execute.</returns>
        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        /// <summary>
        /// Executes the Linq expression
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The execute.</returns>
        public object ExecuteQuery<T>(Expression expression)
        {
            expression = PartialEvaluator.Eval(expression, this.CanBeEvaluatedLocally);

            var translator = new MongoQueryTranslator();
            var results = translator.Translate(expression);

            _results = results;

            var executor = new MongoQueryExecutor(_server, results);
            return executor.Execute<T>();
        }

        /// <summary>
        /// Executes the Linq expression
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
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

        public void Dispose()
        {
            _server.Dispose();
        }

        QueryTranslationResults IMongoQueryResults.TranslationResults
        {
            get { return _results; }
        }

    }
}