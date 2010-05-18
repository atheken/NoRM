using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Collections;

namespace Norm.Linq
{
    /// <summary>
    /// Executes the query against the database
    /// </summary>
    public class MongoQueryExecutor
    {
        private readonly Mongo _mongo;
        private readonly QueryTranslationResults _translationResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryExecutor"/> class.
        /// </summary>
        /// <param name="mongo">The mongo instance</param>
        /// <param name="translationResults">The results of the query translation</param>
        public MongoQueryExecutor(Mongo mongo, QueryTranslationResults translationResults)
        {
            _mongo = mongo;
            _translationResults = translationResults;
        }

        /// <summary>
        /// Performs the query against the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public object Execute<T>()
        {
            // This is the actual Query mechanism...            
            var collection = new MongoCollection<T>(_translationResults.CollectionName, _mongo.Database, _mongo.Database.CurrentConnection);

            string map = "", reduce = "", finalize = "";
            if (!string.IsNullOrEmpty(_translationResults.AggregatePropName))
            {
                map = "function(){emit(0, {val: this." + _translationResults.AggregatePropName + ",tSize:1} )};";
                if (!string.IsNullOrEmpty(_translationResults.Query))
                {
                    map = "function(){if (" + _translationResults.Query + ") {emit(0, {val: this." + _translationResults.AggregatePropName + ",tSize:1} )};}";
                }

                finalize = "function(key, res){ return res.val; }";
            }

            switch (_translationResults.MethodCall)
            {
                case "SingleOrDefault":
                case "FirstOrDefault":
                case "Single":
                case "First":
                    _translationResults.Take = 1;
                    break;
            }

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
                    reduce = "function(key, values){var sum = 0; for(var i = 0; i < values.length; i++){ sum+=values[i].val;} return {val:sum};}";
                    result = ExecuteMapReduce<double>(_translationResults.TypeName, map, reduce, finalize);
                    break;
                case "Average":
                    reduce = "function(key, values){var sum = 0, tot = 0; for(var i = 0; i < values.length; i++){sum += values[i].val; tot += values[i].tSize; } return {val:sum,tSize:tot};}";
                    finalize = "function(key, res){ return res.val / res.tSize; }";
                    result = ExecuteMapReduce<double>(_translationResults.TypeName, map, reduce, finalize);
                    break;
                case "Min":
                    reduce = "function(key, values){var least = 0; for(var i = 0; i < values.length; i++){if(i==0 || least > values[i].val){least=values[i].val;}} return {val:least};}";
                    result = ExecuteMapReduce<double>(_translationResults.TypeName, map, reduce, finalize);
                    break;
                case "Max":
                    reduce = "function(key, values){var least = 0; for(var i = 0; i < values.length; i++){if(i==0 || least < values[i].val){least=values[i].val;}} return {val:least};}";
                    result = ExecuteMapReduce<double>(_translationResults.TypeName, map, reduce, finalize);
                    break;
                default:
                    if (_translationResults.Sort.AllProperties().Count() > 0)
                    {
                        _translationResults.Sort.ReverseKitchen();
                        result = collection.Find(_translationResults.Where, _translationResults.Sort, _translationResults.Take, _translationResults.Skip, collection.FullyQualifiedName);
                    }
                    else
                    {
                        result = collection.Find(_translationResults.Where, _translationResults.Take, _translationResults.Skip);
                    }

                    switch (_translationResults.MethodCall)
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
        
        private T ExecuteMapReduce<T>(string typeName, string map, string reduce, string finalize)
        {
            var mr = _mongo.CreateMapReduce();
            var response = mr.Execute(new MapReduceOptions(typeName) { Map = map, Reduce = reduce, Finalize = finalize });
            var coll = response.GetCollection<MapReduceResult<T>>();
            var r = coll.Find().FirstOrDefault();
            T result = r != null ? r.Value : default(T);

            return result;
        }
    }
}
