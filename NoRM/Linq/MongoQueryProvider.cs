using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NoRM.BSON;

namespace NoRM.Linq {
    public class MongoQueryProvider : IQueryProvider {
        private readonly MongoDatabase _db;
        private readonly Mongo _server;
        private readonly IConnection _connection;
                
        public MongoQueryProvider(string connectionString) {
            _server = new Mongo(connectionString); //todo when is it safe to dispose of this?
            _db = _server.Database;
            _connection = _server.ServerConnection();
        }
        public MongoDatabase DB {
            get {
                return _db;
            }
        }

        public Mongo Server {
            get {
                return _server;
            }
        }
        public IConnection Connection
        {
            get { return _connection; }
        }

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
            return (S)this.Execute<S>(expression);
        }

        object IQueryProvider.Execute(Expression expression) {
            return this.Execute(expression);
        }

        public IEnumerable<T> Execute<T>(Expression expression) {

            //create the collection
            MongoCollection<T> collection = new MongoCollection<T>(typeof(T).Name, this.DB, _connection);

            //pass off the to the translator, which will set the query stuff
            var tranny = new MongoQueryTranslator<T>();
            var qry = tranny.Translate(expression);

            //execute
            if (!String.IsNullOrEmpty(qry)) {
                var fly = new Flyweight();
                fly["$where"] = " function(){return "+ qry+"; }";
                return collection.Find(fly);
            }

            return collection.Find();

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
