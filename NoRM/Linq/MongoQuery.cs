namespace NoRM.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    
    public class MongoQuery<T> : IOrderedQueryable<T>
    {
        private readonly Expression _expression;
        private readonly MongoQueryProvider _provider;

        public MongoQuery(MongoQueryProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            _provider = provider;
            _expression = Expression.Constant(this);        
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
            _provider = provider;
            _expression = expression;            
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
            return _provider.Execute<T>(_expression).GetEnumerator();
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