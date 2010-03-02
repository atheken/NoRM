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
            return (S)this.Execute(expression);

        }

        object IQueryProvider.Execute(Expression expression) {
            
            return this.Execute(expression);
        }

        public IEnumerable<T> Execute<T>(Expression expression) {

            //create the collection
            MongoCollection<T> collection = new MongoCollection<T>(typeof(T).Name, this.DB, this.Server);
            expression = PartialEvaluator.Eval(expression);

            //pass off the to the translator, which will set the query stuff
            var tranny = new MongoQueryTranslator();
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

            //this is called from things OTHER than Enumerable() - like ToList() etc
            //create the collection
            var tranny = new MongoQueryTranslator();
            var qry = tranny.Translate(expression);

            Flyweight fly = (Flyweight)tranny.FlyWeight;
            var collection = new MongoCollection(fly.TypeName,this.DB,this.Server);

            if (!String.IsNullOrEmpty(qry)) {
                fly["$where"] = " function(){return " + qry + "; }";
            }

            object result = null;
            //what's the method call?
            switch(fly.MethodCall){
                case "Count":
                    result = collection.Count(fly);
                    break;
                default:
                    break;

            }
            return result;
            //this has a method call associated with it - which one?
        }

    }
}
