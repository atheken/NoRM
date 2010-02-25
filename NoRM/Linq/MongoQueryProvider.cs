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
        private MongoDatabase _db;
        private MongoServer _server;
        
        public MongoQueryProvider(string dbName) : this(dbName, "127.0.0.1", 27017, false) { }
        public MongoQueryProvider(string dbName, string server, int port, bool enableExpandoProps) {

            _server = new MongoServer(server,port,enableExpandoProps);
            _db = _server.GetDatabase(dbName);

        }
        public MongoDatabase DB {
            get {
                return _db;
            }
        }

        public MongoServer Server {
            get {
                return _server;
            }
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

            //this is called from things OTHER than Enumerable() - like ToList() etc
            //create the collection
            MongoCollection<S> collection = new MongoCollection<S>(typeof(S).Name, this.DB, this.Server);
            var tranny = new MongoQueryTranslator<S>();
            var qry = tranny.Translate(expression);
            Flyweight fly = (Flyweight)tranny.FlyWeight;

            switch (fly.MethodCall) {
                case "Count":
                    fly["count"] = typeof(S).Name;
                    break;
                default:
                    break;
            }


            if (!String.IsNullOrEmpty(qry)) {
                fly["$where"] = " function(){return " + qry + "; }";
            }

            //this has a method call associated with it - which one?
            return collection.FindOne(fly);

        }

        object IQueryProvider.Execute(Expression expression) {
            return this.Execute(expression);
        }

        public IEnumerable<T> Execute<T>(Expression expression) {

            //create the collection
            MongoCollection<T> collection = new MongoCollection<T>(typeof(T).Name, this.DB, this.Server);
            expression = PartialEvaluator.Eval(expression);

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
