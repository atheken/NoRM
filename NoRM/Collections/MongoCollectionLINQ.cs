//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Collections;
//using Norm.Linq;
//using System.Linq.Expressions;

//namespace Norm.Collections
//{
//    public partial class MongoCollection<T> : IMongoCollection<T>, IQueryable<T>, IQueryable,
//        IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable, IMongoQuery
//    {
//        private MongoQuery<T> _queryContext;

//        public Expression GetExpression()
//        {
//            return _queryContext.GetExpression();
//        }

//        public Type ElementType
//        {
//            get { return ((IQueryable<T>)_queryContext).ElementType; }
//        }

//        public Expression Expression
//        {
//            get { return ((IQueryable<T>)_queryContext).Expression; }
//        }

//        public IQueryProvider Provider
//        {
//            get { return ((IQueryable<T>)this._queryContext).Provider; }
//        }

//        public IEnumerator GetEnumerator()
//        {
//            return this._queryContext.GetEnumerator();
//        }

//        IEnumerator<T> IEnumerable<T>.GetEnumerator()
//        {
//            return ((IEnumerable<T>)this._queryContext).GetEnumerator();
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return ((IEnumerable)this._queryContext).GetEnumerator();
//        }

//    }
//}