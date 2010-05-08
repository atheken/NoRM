using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Norm.BSON;
using Norm.Collections;
using System.Collections;
using Norm.Configuration;

namespace Norm.Linq
{
    /// <summary>
    /// The mongo query provider.
    /// </summary>
    public class MongoQueryProvider : IQueryProvider, IDisposable
    {
        private readonly Mongo _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryProvider"/> class.
        /// </summary>
        /// <param name="server">
        /// The server.
        /// </param>
        public MongoQueryProvider(Mongo server)
        {
            _server = server;
        }

        public static MongoQueryProvider Create(String connectionString)
        {
            return new MongoQueryProvider(Mongo.Create(connectionString));
        }

        /// <summary>
        /// Gets the DB.
        /// </summary>
        public MongoDatabase DB
        {
            get { return _server.Database; }
        }

        /// <summary>
        /// Gets the server.
        /// </summary>
        public Mongo Server
        {
            get { return _server; }
        }

        /// <summary>
        /// The i query provider. create query.
        /// </summary>
        /// <typeparam name="S">Type of query to create</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            var query = new MongoQuery<S>(this, expression);
            return query;
        }

        /// <summary>
        /// The i query provider. create query.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable"/> that can evaluate the query represented by the specified expression tree.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            var elementType = LinqTypeHelper.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(MongoQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// The i query provider. execute.
        /// </summary>
        /// <typeparam name="S">Type to execute</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>Resulting object</returns>
        S IQueryProvider.Execute<S>(Expression expression)
        {
            object result = ExecuteQuery<S>(expression);
            return (S)Convert.ChangeType(result, typeof(S));
        }

        /// <summary>
        /// The i query provider. execute.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The i query provider. execute.</returns>
        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        /// <summary>
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The execute.</returns>
        public object ExecuteQuery<T>(Expression expression)
        {
            expression = PartialEvaluator.Eval(expression, this.CanBeEvaluatedLocally);

            var translator = new MongoQueryTranslator();
            var qry = translator.Translate(expression);
            var fly = translator.FlyWeight;

            // This is the actual Query mechanism...            
            var collection = new MongoCollection<T>(translator.CollectionName, DB, DB.CurrentConnection);

            string map = "", reduce = "", finalize = "";
            if (!string.IsNullOrEmpty(translator.AggregatePropName))
            {
                map = "function(){emit(0, {val: this." + translator.AggregatePropName + ",tSize:1} )};";
                if (!string.IsNullOrEmpty(qry))
                {
                    map = "function(){if (" + qry + ") {emit(0, {val: this." + translator.AggregatePropName + ",tSize:1} )};}";
                }

                finalize = "function(key, res){ return res.val; }";
            }

            switch (translator.MethodCall)
            {
                case "SingleOrDefault":
                case "FirstOrDefault":
                case "Single":
                case "First":
                    translator.Take = 1;
                    break;
            }

            object result;
            switch (translator.MethodCall)
            {
                case "Any":
                    result = collection.Count(fly) > 0;
                    break;
                case "Count":
                    result = collection.Count(fly);
                    break;
                case "Sum":
                    reduce = "function(key, values){var sum = 0; for(var i = 0; i < values.length; i++){ sum+=values[i].val;} return {val:sum};}";
                    result = ExecuteMR<double>(translator.TypeName, map, reduce, finalize);
                    break;
                case "Average":
                    reduce = "function(key, values){var sum = 0, tot = 0; for(var i = 0; i < values.length; i++){sum += values[i].val; tot += values[i].tSize; } return {val:sum,tSize:tot};}";
                    finalize = "function(key, res){ return res.val / res.tSize; }";
                    result = ExecuteMR<double>(translator.TypeName, map, reduce, finalize);
                    break;
                case "Min":
                    reduce = "function(key, values){var least = 0; for(var i = 0; i < values.length; i++){if(i==0 || least > values[i].val){least=values[i].val;}} return {val:least};}";
                    result = ExecuteMR<double>(translator.TypeName, map, reduce, finalize);
                    break;
                case "Max":
                    reduce = "function(key, values){var least = 0; for(var i = 0; i < values.length; i++){if(i==0 || least < values[i].val){least=values[i].val;}} return {val:least};}";
                    result = ExecuteMR<double>(translator.TypeName, map, reduce, finalize);
                    break;
                default:
                    if (translator.SortFly.AllProperties().Count() > 0)
                    {
                        translator.SortFly.ReverseKitchen();
                        result = collection.Find(fly, translator.SortFly, translator.Take, translator.Skip, collection.FullyQualifiedName);
                    }
                    else
                    {
                        result = collection.Find(fly, translator.Take, translator.Skip);
                    }

                    switch (translator.MethodCall)
                    {
                        case "SingleOrDefault": result = ((IEnumerable<T>)result).SingleOrDefault(); break;
                        case "FirstOrDefault": result = ((IEnumerable<T>)result).FirstOrDefault(); break;
                        case "Single": result = ((IEnumerable<T>)result).Single(); break;
                        case "First": result = ((IEnumerable<T>)result).First(); break;
                    }

                    break;
            }

            return result;
        }

        public object Execute(Expression expression)
        {
            var elementType = LinqTypeHelper.GetElementType(expression.Type);
            try
            {
                return typeof(MongoQueryProvider)
                    .GetMethod("ExecuteQuery")
                    .MakeGenericMethod(elementType)
                    .Invoke(this, new object[] { expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        private bool CanBeEvaluatedLocally(Expression expression)
        {
            // any operation on a query can't be done locally
            ConstantExpression cex = expression as ConstantExpression;
            if (cex != null)
            {
                IQueryable query = cex.Value as IQueryable;
                if (query != null && query.Provider == this)
                    return false;
            }
            MethodCallExpression mc = expression as MethodCallExpression;
            if (mc != null &&
                (mc.Method.DeclaringType == typeof(Enumerable) ||
                 mc.Method.DeclaringType == typeof(Queryable)))
            {
                return false;
            }
            if (expression.NodeType == ExpressionType.Convert &&
                expression.Type == typeof(object))
                return true;
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }

        /// <summary>
        /// Actual execution of the MapReduce stuff (thanks Karl!)
        /// </summary>
        /// <typeparam name="T">Type to map and reduce</typeparam>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="map">The map.</param>
        /// <param name="reduce">The reduce.</param>
        /// <param name="finalize">The finalize.</param>
        /// <returns></returns>
        private T ExecuteMR<T>(string typeName, string map, string reduce, string finalize)
        {
            var mr = Server.CreateMapReduce();
            var response = mr.Execute(new MapReduceOptions(typeName) {Map = map, Reduce = reduce, Finalize = finalize});
            var coll = response.GetCollection<MapReduceResult<T>>();
            var r = coll.Find().FirstOrDefault();
            T result = r.Value;
            
            return result;
        }

        public void Dispose()
        {
            _server.Dispose();
        }
    }
}