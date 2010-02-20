using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections;

namespace NoRM.Linq {
    /// <summary>
    /// A default implementation of IQueryable for use with QueryProvider
    /// </summary>
    public class MongoQuery<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable {
        MongoQueryProvider provider;
        Expression expression;

        public MongoQuery(MongoQueryProvider provider) {
            if (provider == null) {
                throw new ArgumentNullException("provider");
            }
            this.provider = provider;
            this.expression = Expression.Constant(this);
        }

        public MongoQuery(MongoQueryProvider provider, Expression expression) {
            if (provider == null) {
                throw new ArgumentNullException("provider");
            }
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type)) {
                throw new ArgumentOutOfRangeException("expression");
            }
            this.provider = provider;
            this.expression = expression;
        }

        Expression IQueryable.Expression {
            get { return this.expression; }
        }

        Type IQueryable.ElementType {
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider {
            get { return this.provider; }
        }

        public virtual IEnumerator<T> GetEnumerator() {
            //var docs= (ICursor)this.provider.Execute(this.expression);

            //IEnumerable<T> result = docs.TranslateDocs<T>();

            //return result.GetEnumerator();
            return null;

        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)this.provider.Execute(this.expression)).GetEnumerator();
        }

        public override string ToString() {
            if (this.expression.NodeType == ExpressionType.Constant &&
                ((ConstantExpression)this.expression).Value == this) {
                return "Query(" + typeof(T) + ")";
            } else {
                return this.expression.ToString();
            }
        }

    }
}
