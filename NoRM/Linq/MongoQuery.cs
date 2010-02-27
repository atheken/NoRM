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
        MongoQueryProvider provider;
        Expression expression;
        MongoCollection<T> _collection;
        Flyweight _query;

        public MongoQuery(MongoQueryProvider provider, String collectionName)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this.provider = provider;
            this.expression = Expression.Constant(this);
            _collection = new MongoCollection<T>(collectionName, provider.DB, provider.Server);
            _query = new Flyweight();
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
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
            this.provider = provider;
            this.expression = expression;
            _collection = new MongoCollection<T>(typeof(T).Name, provider.DB, provider.Server);
        }

        Expression IQueryable.Expression
        {
            get { return this.expression; }
        }

        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this.provider; }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            //var docs= (ICursor)this.provider.Execute(this.expression);
            return provider.Execute<T>(this.expression).GetEnumerator();

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.provider.Execute(this.expression)).GetEnumerator();
        }

        public override string ToString()
        {
            if (this.expression.NodeType == ExpressionType.Constant &&
                ((ConstantExpression)this.expression).Value == this)
            {
                return "Query(" + typeof(T) + ")";
            }
            else
            {
                return this.expression.ToString();
            }
        }

    }
}
