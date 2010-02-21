using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Linq;
using System.Linq.Expressions;

namespace NoRM
{
    public partial class MongoCollection<T> : IQueryable<T>, IQueryable, IOrderedQueryable<T>, IOrderedQueryable
    {
        private MongoQuery<T> _queryable;
        protected MongoQuery<T> Queryable
        {
            get
            {
                this._queryable = this._queryable ?? new MongoQuery<T>(new MongoQueryProvider(this._db.DatabaseName));
                return this._queryable;
            }
        }

        Expression IQueryable.Expression
        {
            get { return ((IQueryable)this.Queryable).Expression; }
        }

        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return ((IQueryable)this.Queryable).Provider; }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.Queryable.GetEnumerator();
        }


        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.Queryable.GetEnumerator();
        }

    }
}
