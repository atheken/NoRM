using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Norm.BSON;
using Norm.Collections;

namespace Norm.Linq
{
    /// <summary>
    /// The mongo query provider.
    /// </summary>
    public class MongoQueryProvider : IQueryProvider
    {
        private readonly Mongo _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryProvider"/> class.
        /// </summary>
        /// <param name="dbName">
        /// The db name.
        /// </param>
        public MongoQueryProvider(string dbName)
            : this(dbName, "127.0.0.1", "27017", string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryProvider"/> class.
        /// </summary>
        /// <param name="dbName">The db name.</param>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        /// <param name="options">The options.</param>
        public MongoQueryProvider(string dbName, string server, string port, string options)
            : this(new Mongo(dbName, server, port, options))
        {
        }

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
        /// <returns></returns>
        S IQueryProvider.Execute<S>(Expression expression)
        {
            // This duplication of code sucks - I'll refactor it out 
            // but right now the type conversion is SUCK

            var m = (MethodCallExpression)expression;
            if (m.Method.Name.StartsWith("First") || m.Method.Name.StartsWith("Single"))
            {
                var collection = DB.GetCollection<S>();
                var tranny = new MongoQueryTranslator();
                var qry = tranny.Translate(expression);
                var fly = tranny.FlyWeight;

                if (!string.IsNullOrEmpty(qry))
                {
                    if (tranny.IsComplex)
                    {
                        fly = new Flyweight();
                        fly["$where"] = " function(){return " + qry + "; }";
                    }
                }

                return collection.FindOne(fly);
            }
           
            var result = Execute(expression);
            var converted = (S)Convert.ChangeType(result, typeof(S));
            return converted;
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
        /// The execute.
        /// </summary>
        /// <typeparam name="T">Type to execute</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public IEnumerable<T> Execute<T>(Expression expression)
        {
            // create the collection
            var collection = DB.GetCollection<T>();
            expression = PartialEvaluator.Eval(expression);

            // pass off the to the translator, which will set the query stuff
            var tranny = new MongoQueryTranslator();
            var qry = tranny.Translate(expression);
            var fly = tranny.FlyWeight;


            // execute
            if (!string.IsNullOrEmpty(qry))
            {
                if (tranny.IsComplex)
                {
                    // reset - need to use the where statement generated
                    // instead of the props set on the internal flyweight
                    fly = new Flyweight();
                    fly["$where"] = " function(){return " + qry + "; }";
                }
            }

            return collection.Find(fly);
        }

        /// <summary>
        /// Avert your eyes all who come here seeking enlightenment. You cannot behold the true
        /// joy and intense power of the treasure lying inside the mess. So I beseech you - turn
        /// back now if you are queasy, squeemish, or otherwise feel entitled to code that not
        /// only does what you need, but looks pretty doing it.
        /// I'll clean this up as I have time. For now, it's vomitous.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The execute.</returns>
        public object Execute(Expression expression)
        {
            // this is called from things OTHER than Enumerable() - like ToList() etc
            // create the collection
            var tranny = new MongoQueryTranslator();

            // need to know what method was called here. This corresponds to functions 
            // like "Sum, Join," etc. It's the last thing in the method chain
            // Where gets sent to the Enumeration method, as does Select etc.
            // pull out the MethodCallExpression
            var m = expression as MethodCallExpression;

            // The query itself is the first arg of the main expression. This is what we need to eval.
            var qry = tranny.Translate(m.Arguments[0]);
            var fly = tranny.FlyWeight;

            // set the method call to the Method name. Yeehaa.
            tranny.MethodCall = m.Method.Name;

            // if it's here, we need it - the last arg is the lambda passed in as an argument to the method
            // this will give us the prop name for our mapreduce action
            if (m.Arguments.Count > 1)
            {
                if (tranny.MethodCall == "Any")
                {
                    // any has a boolean lambda as a body - translate this 
                    // as normal
                    qry = tranny.Translate(m.Arguments[1]);


                    // the property name doesn't matter with Any
                }
                else
                {
                    tranny.PropName = tranny.Translate(m.Arguments[1]);
                }
            }
            else
            {
                // it's a straight query call - grab from first
                tranny.PropName = tranny.Translate(m.Arguments[0]);
            }

            // the type we're dealing with is a ConstantExpression, hanging off the 
            // first argument. Need to set it so our little fly guy
            // knows what collection to use
            tranny.TypeName = tranny.TranslateCollectionName(m.Arguments[0]);


            // This is the actual Query mechanism...
            var collection = new MongoCollection(tranny.TypeName, DB, DB.CurrentConnection);

            // if a query comes back, create a $where. We'll use this for Count() 
            // and Group in the future.
            if (!string.IsNullOrEmpty(qry))
            {
                if (tranny.IsComplex)
                {
                    fly = new Flyweight();
                    fly["$where"] = " function(){return " + qry + "; }";
                }
            }

            // This is the mapping function (javascript. Yummy).
            // It defines our collection that we'll iterate (reduce) over
            // James, you're an animal.
            var averyMap = "function(){emit(0, " + tranny.PropName + ")};";

            // if they pass in a query (Where()), we need to graft that on
            // to our Mapping. You can thank Avery for this one.
            if (!string.IsNullOrEmpty(qry))
            {
                averyMap = "function(){if" + qry + "{emit(0, " + tranny.PropName + ")};}";
            }

            var reduce = string.Empty;
            object result = null;

            // Whachoo SAY!
            switch (tranny.MethodCall)
            {
                // I'm Just ASKIN!!!
                case "Any":
                    result = collection.Count(tranny.FlyWeight) > 0;
                    break;
                case "Count":
                    result = collection.Count(fly);
                    break;
                case "Sum":
                    reduce = "function(key, values){var sum = 0;for(var i in values){ sum+=values[i];} return sum;}";
                    result = ExecuteMR<long>(tranny.TypeName, averyMap, reduce);
                    break;
                case "Average":
                    reduce =
                        "function(key, values){var sum = 0; for(var i = 0; i < values.length; ++i){sum += values[i]} return sum/values.length;}";
                    result = ExecuteMR<long>(tranny.TypeName, averyMap, reduce);
                    break;
                case "Min":
                    reduce =
                        "function(key, values){var least = 0; for(var i = 0; i < values.length; ++i){if(i==0 || least > values[i]){least=values[i];}} return least;}";
                    result = ExecuteMR<long>(tranny.TypeName, averyMap, reduce);
                    break;
                case "Max":
                    reduce =
                        "function(key, values){var least = 0; for(var i = 0; i < values.length; ++i){if(i==0 || least < values[i]){least=values[i];}} return least;}";
                    result = ExecuteMR<long>(tranny.TypeName, averyMap, reduce);
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// Actual execution of the MapReduce stuff (thanks Karl!)
        /// </summary>
        /// <typeparam name="T">Type to map and reduce</typeparam>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="map">The map.</param>
        /// <param name="reduce">The reduce.</param>
        /// <returns></returns>
        private T ExecuteMR<T>(string typeName, string map, string reduce)
        {
            T result;
            using (var mr = Server.CreateMapReduce())
            {
                var response = mr.Execute(new MapReduceOptions(typeName) { Map = map, Reduce = reduce });
                var coll = response.GetCollection<MapReduceResult<T>>();
                var r = coll.Find().FirstOrDefault();
                result = r.Value;
            }

            return result;
        }
    }
}