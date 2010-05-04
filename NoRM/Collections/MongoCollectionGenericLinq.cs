using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;

namespace Norm.Collections
{
    public partial class MongoCollection<T> : IMongoCollection<T>, IQueryable<T>
    {
        //the LINQ passthrough stuff.
        private IQueryable<T> _queryContext;

        public IEnumerator<T> GetEnumerator()
        {
            return this._queryContext.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._queryContext.GetEnumerator();
        }

        public Type ElementType
        {
            get { return this._queryContext.ElementType; }
        }

        public Expression Expression
        {
            get { return this._queryContext.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return this._queryContext.Provider; }
        }
    }
}
