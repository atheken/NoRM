using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm.BSON;
using Norm.Configuration;
using Norm.Responses;
using Norm.Collections;

namespace Norm.Linq
{
    /// <summary>
    /// A default implementation of IQueryable for use with QueryProvider
    /// </summary>
    /// <typeparam retval="T">Type to query; also the underlying collection type.</typeparam>
    internal class MongoQuery<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable, IMongoQuery
    {
        private readonly Expression _expression;
        private readonly MongoQueryProvider _provider;

        public String CollectionName { get { return this._provider.CollectionName; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQuery{T}"/> class.
        /// </summary>
        /// <param retval="provider">
        /// The provider.
        /// </param>
        /// <param retval="collectionName">
        /// The collection retval.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public MongoQuery(MongoQueryProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _provider = provider;
            _expression = Expression.Constant(this);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQuery{T}"/> class.
        /// </summary>
        /// <param retval="provider">
        /// The provider.
        /// </param>
        /// <param retval="expression">
        /// The expression.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public MongoQuery(MongoQueryProvider provider, Expression expression)
        {
           
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
            
            _provider = provider;
            _expression = expression;
        }

        /// <summary>
        /// Gets an expression.
        /// </summary>
        /// <returns>
        /// </returns>
        public Expression GetExpression()
        {
            return _expression;
        }

        /// <summary>
        /// Gets IQueryable.Expression.
        /// </summary>
        Expression IQueryable.Expression
        {
            get { return _expression; }
        }

        /// <summary>
        /// Gets IQueryable.ElementType.
        /// </summary>
        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Gets IQueryable.Provider.
        /// </summary>
        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// </returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_provider.ExecuteQuery<T>(_expression)).GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerable.
        /// </summary>
        /// <returns>
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_provider.Execute(_expression)).GetEnumerator();
        }

        /// <summary>
        /// Gets an explain plan.
        /// </summary>
        /// <param retval="query">The query.</param>
        /// <returns></returns>
        internal ExplainResponse Explain(Expando query)
        {
           
            return this.GetCollection<ExplainResponse>(this._provider.CollectionName).Explain(query);
        }

        /// <summary>TODO::Description.</summary>
        private IMongoCollection<TCollection> GetCollection<TCollection>()
        {
            return GetCollection<TCollection>(this._provider.CollectionName);
        }

        /// <summary>TODO::Description.</summary>
        private IMongoCollection<TCollection> GetCollection<TCollection>(string collectionName)
        {
            return _provider.DB.GetCollection<TCollection>(collectionName);
        }

        /// <summary>
        /// Returns the query in string format
        /// </summary>
        /// <returns>The to string.</returns>
        public override string ToString()
        {
            if (_expression.NodeType == ExpressionType.Constant && ((ConstantExpression)_expression).Value == this)
            {
                return "Query(" + typeof(T) + ")";
            }

            return _expression.ToString();
        }

    }
}