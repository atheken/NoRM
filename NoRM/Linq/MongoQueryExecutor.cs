using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Collections;
using System.Reflection;
using Norm.BSON;
using System.Linq.Expressions;
using System.Collections;

namespace Norm.Linq
{
    /// <summary>
    /// Executes the query against the database
    /// </summary>
    internal class MongoQueryExecutor
    {
        private readonly IMongoDatabase _db;
        private readonly QueryTranslationResults _translationResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryExecutor"/> class.
        /// </summary>
        /// <param retval="mongo">The database on which the query will be executed.</param>
        /// <param retval="translationResults">The results of the query translation</param>
        public MongoQueryExecutor(IMongoDatabase db, QueryTranslationResults translationResults)
        {
            _db = db;
            _translationResults = translationResults;
        }

        /// <summary>
        /// Performs the query against the database
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <returns></returns>
        public object Execute<T>()
        {
            // This is the actual Query mechanism...            
            IMongoCollection<T> collection = new MongoCollection<T>(_translationResults.CollectionName, _db, _db.CurrentConnection);

            object result;
            switch (_translationResults.MethodCall)
            {
                case "Any":
                    result = collection.Count(_translationResults.Where) > 0;
                    break;
                case "Count":
                    result = collection.Count(_translationResults.Where);
                    break;
                case "Sum":
                    result = ExecuteMapReduce<double>(_translationResults.TypeName, BuildSumMapReduce());
                    break;
                case "Average":
                    result = ExecuteMapReduce<double>(_translationResults.TypeName, BuildAverageMapReduce());
                    break;
                case "Min":
                    result = ExecuteMapReduce<double>(_translationResults.TypeName, BuildMinMapReduce());
                    break;
                case "Max":
                    result = ExecuteMapReduce<double>(_translationResults.TypeName, BuildMaxMapReduce());
                    break;
                default:
                    _translationResults.Take = IsSingleResultMethod(_translationResults.MethodCall) ? 1 : _translationResults.Take;
                    _translationResults.Sort.ReverseKitchen();

                    if (_translationResults.Select == null)
                    {
                        result = collection.Find(_translationResults.Where, _translationResults.Sort, _translationResults.Take, _translationResults.Skip, collection.FullyQualifiedName);
                        switch (_translationResults.MethodCall)
                        {
                            case "SingleOrDefault": result = ((IEnumerable<T>)result).SingleOrDefault(); break;
                            case "Single": result = ((IEnumerable<T>)result).Single(); break;
                            case "FirstOrDefault": result = ((IEnumerable<T>)result).FirstOrDefault(); break;
                            case "First": result = ((IEnumerable<T>)result).First(); break;
                        }
                    }
                    else
                    {
                        Type t = collection.GetType();
                        MethodInfo mi = t.GetMethod("FindFieldSelection", BindingFlags.Instance | BindingFlags.NonPublic);
                        var sortType = _translationResults.Sort ?? new Object();
                        Type[] argTypes = { typeof(Expando), sortType.GetType(), _translationResults.Select.Body.Type };
                        MethodInfo method = mi.MakeGenericMethod(argTypes);
                        result = method.Invoke(collection, new object[]{_translationResults.Where,
                            _translationResults.Sort, 
                            _translationResults.Take,
                            _translationResults.Skip,
                            collection.FullyQualifiedName,
                            _translationResults.Select});

                        switch (_translationResults.MethodCall)
                        {
                            case "SingleOrDefault": result = ((IEnumerable)result).OfType<Object>().SingleOrDefault(); break;
                            case "Single": result = ((IEnumerable)result).OfType<Object>().Single(); break;
                            case "FirstOrDefault": result = ((IEnumerable)result).OfType<Object>().FirstOrDefault(); break;
                            case "First": result = ((IEnumerable)result).OfType<Object>().First(); break;
                        }

                    }
                    break;
            }

            return result;
        }

        private MapReduceParameters InitializeDefaultMapReduceParameters()
        {
            var map = "";
            var finalize = "";

            if (!string.IsNullOrEmpty(_translationResults.AggregatePropName))
            {
                map = "function(){emit(0, {val:this." + _translationResults.AggregatePropName + ",tSize:1} )};";
                if (!string.IsNullOrEmpty(_translationResults.Query))
                {
                    map = "function(){if (" + _translationResults.Query + ") {emit(0, {val:this." + _translationResults.AggregatePropName + ",tSize:1} )};}";
                }

                finalize = "function(key, res){ return res.val; }";
            }

            return new MapReduceParameters { Map = map, Reduce = string.Empty, Finalize = finalize };
        }

        private MapReduceParameters BuildSumMapReduce()
        {
            var parameters = InitializeDefaultMapReduceParameters();
            parameters.Reduce = "function(key, values){var sum = 0; for(var i = 0; i < values.length; i++){ sum+=values[i].val;} return {val:sum};}";

            return parameters;
        }

        private MapReduceParameters BuildAverageMapReduce()
        {
            var parameters = InitializeDefaultMapReduceParameters();

            parameters.Reduce = "function(key, values){var sum = 0, tot = 0; for(var i = 0; i < values.length; i++){sum += values[i].val; tot += values[i].tSize; } return {val:sum,tSize:tot};}";
            parameters.Finalize = "function(key, res){ return res.val / res.tSize; }";

            return parameters;
        }

        private MapReduceParameters BuildMinMapReduce()
        {
            var parameters = InitializeDefaultMapReduceParameters();
            parameters.Reduce = "function(key, values){var least = 0; for(var i = 0; i < values.length; i++){if(i==0 || least > values[i].val){least=values[i].val;}} return {val:least};}";

            return parameters;
        }

        private MapReduceParameters BuildMaxMapReduce()
        {
            var parameters = InitializeDefaultMapReduceParameters();
            parameters.Reduce = "function(key, values){var least = 0; for(var i = 0; i < values.length; i++){if(i==0 || least < values[i].val){least=values[i].val;}} return {val:least};}";

            return parameters;
        }

        private T ExecuteMapReduce<T>(string typeName, MapReduceParameters parameters)
        {
            var mr = _db.CreateMapReduce();
            var response = mr.Execute(new MapReduceOptions(typeName) { Map = parameters.Map, Reduce = parameters.Reduce, Finalize = parameters.Finalize });
            var coll = response.GetCollection<MapReduceResult<T>>();
            var r = coll.Find().FirstOrDefault();
            T result = r != null ? r.Value : default(T);

            return result;
        }

        private bool IsAggregateMethod(string method)
        {
            return (new[] { "Min", "Max", "Average", "Sum" }).Contains(method);
        }

        private bool IsSingleResultMethod(string method)
        {
            return (new[] { "Single", "SingleOrDefault", "First", "FirstOrDefault" }).Contains(method);
        }
    }
}
