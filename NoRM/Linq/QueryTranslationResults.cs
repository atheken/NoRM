using Norm.BSON;

namespace Norm.Linq
{
    /// <summary>
    /// Holds information gathered from the Linq Translator
    /// </summary>
    public class QueryTranslationResults
    {
        public Expando Where { get; set; }
        public Expando Sort { get; set; }
        
        public string AggregatePropName { get; set; }
        public string CollectionName { get; set; }
        public string MethodCall { get; set; }
        public string TypeName { get; set; }

        public int Skip { get; set; }
        public int Take { get; set; }

        public bool IsComplex { get; set; }

        public string Query { get; set; }
    }
}