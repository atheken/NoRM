using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm.BSON;
using Norm.Configuration;
using Norm.Responses;

namespace Norm.Linq
{
    /// <summary>
    /// A default implementation of IQueryable for use with QueryProvider
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class MongoQuery<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable, IMongoQuery
    {
        private MongoCollection<T> _collection;
        private Expression _expression;
        private MongoQueryProvider _provider;
        private Flyweight _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQuery{T}"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="collectionName">
        /// The collection name.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public MongoQuery(MongoQueryProvider provider, string collectionName)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _provider = provider;
            _expression = Expression.Constant(this);
            _collection = provider.DB.GetCollection<T>();
            _query = new Flyweight();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQuery{T}"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public MongoQuery(MongoQueryProvider provider) : this(provider, typeof(T).Name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQuery{T}"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="expression">
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
            // var docs= (ICursor)this.provider.Execute(this.expression);
            return _provider.Execute<T>(_expression).GetEnumerator();
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
        /// <param name="query">The query.</param>
        /// <returns></returns>
        internal ExplainResponse Explain(string query)
        {
            var queryFlyweight = new Flyweight
                                     {
                                         MethodCall = query
                                     };

            // I have no idea if $query should be another flyweight, a raw string, or what.
            var explain = new Flyweight();
            explain["$query"] = queryFlyweight;
            explain["$explain"] = true;

            var collectionName = MongoConfiguration.GetCollectionName(typeof(T));
            return _provider.DB.GetCollection<ExplainResponse>(collectionName).FindOne(explain);
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