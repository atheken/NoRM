using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NoRM.Linq {
    public class MongoQueryProvider : IQueryProvider {

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression) {
            var query= new MongoQuery<S>(this, expression);
            return query;
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression) {
            Type elementType = TypeHelper.GetElementType(expression.Type);
            try {
                return (IQueryable)Activator.CreateInstance(typeof(MongoQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            } catch (TargetInvocationException tie) {
                throw tie.InnerException;
            }
        }

        S IQueryProvider.Execute<S>(Expression expression) {
            return (S)this.Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression) {
            return this.Execute(expression);
        }

        public object Execute(Expression expression) {

           // var spec = MongoQueryTranslator.Translate(expression);

            //get the goods from the DB..
            //ICursor cursor;
            //if (spec.Keys.Count==0) {
            //    cursor = _collection.FindAll();
            //} else {
            //    cursor = _collection.Find(spec);
            //}
            //return cursor;
            return null;
        }
    }
}
