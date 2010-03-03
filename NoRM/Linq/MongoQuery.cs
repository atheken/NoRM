using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections;
using NoRM.BSON;

namespace NoRM.Linq
{
    /// <summary>
    /// A default implementation of IQueryable for use with QueryProvider
    /// </summary>
    public class MongoQuery<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {
        MongoQueryProvider _provider;
        Expression _expression;
        MongoCollection<T> _collection;
        Flyweight _query;

        public MongoQuery(MongoQueryProvider provider, String collectionName)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this._provider = provider;
            this._expression = Expression.Constant(this);
            this._collection = provider.DB.GetCollection<T>();
            this._query = new Flyweight();
        }

        public MongoQuery(MongoQueryProvider provider)
            : this(provider, typeof(T).Name)
        {

        }

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
            if (!typeof (IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
            this._provider = provider;
            this._expression = expression;            
        }

        Expression IQueryable.Expression
        {
            get { return _expression; }
        }
        
        Type IQueryable.ElementType
        {
            get { return typeof (T); }
        }
        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            //var docs= (ICursor)this.provider.Execute(this.expression);
            return this._provider.Execute<T>(this._expression).GetEnumerator();

        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _provider.Execute(_expression)).GetEnumerator();
        }

        
        public override string ToString()
        {
            if (_expression.NodeType == ExpressionType.Constant && ((ConstantExpression) _expression).Value == this)
            {
                return "Query(" + typeof (T) + ")";
            }
            return _expression.ToString();
        }
    }
}