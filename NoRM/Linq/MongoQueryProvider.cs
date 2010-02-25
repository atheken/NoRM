namespace NoRM.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using BSON;

    public class MongoQueryProvider : IQueryProvider, IDisposable
    {
        private bool _disposed;
        private readonly Mongo _mongo;
        
        public MongoQueryProvider(string connectionString)
        {
            _mongo = new Mongo(connectionString);
        }

        public Mongo Mongo
        {
            get { return _mongo; }
        }
 
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            var query = new MongoQuery<S>(this, expression);
            return query;
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var elementType = TypeHelper.GetElementType(expression.Type);
            try
            {
                return (IQueryable) Activator.CreateInstance(typeof (MongoQuery<>).MakeGenericType(elementType), new object[] {this, expression});
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S) Execute<S>(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }        

        public IEnumerable<T> Execute<T>(Expression expression)
        {
            var collection = new MongoCollection<T>(typeof(T).Name, _mongo.Database, _mongo.ServerConnection());

            //pass off the to the translator, which will set the query stuff
            var tranny = new MongoQueryTranslator<T>();
            var qry = tranny.Translate(expression);

            //execute
            if (!String.IsNullOrEmpty(qry))
            {
                var fly = new Flyweight();
                fly["$where"] = qry;
                return collection.Find(fly);
            }

            return collection.Find();
            
        }

        public object Execute(Expression expression)
        {
            return null;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _mongo.Dispose();
            }
            _disposed = true;
        }
        ~MongoQueryProvider()
        {
            Dispose(false);
        }

    }
}