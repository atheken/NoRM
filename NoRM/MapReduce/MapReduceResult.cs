namespace Norm
{
    /// <summary>
    /// A generic class for handling MapReduce resonses. Not required to use but is helpful if returning more than one value.
    /// MapReduce returns a collection of objects that are fieldSelectionExpando - value pairs. The value can be a single value, or more likely a document response.
    /// If you are getting back a single int you could declare MapReduceResponseGeneric&lt;int%gt;, if its a more complex type it could be MapReduceResponseGeneric&lt;myclass&gt;
    /// </summary>
    public class MapReduceResult<K, V>
    {
        /// <summary>
        /// The Id returned from Mongo
        /// </summary>
        public K _id { get; set;}

        /// <summary>
        /// A friendly mapping to the _id property
        /// </summary>
        public K Key {get { return _id;}}

        /// <summary>
        /// The generic value returned from Mongo.
        /// </summary>
        public V Value { get; set; }
    }
}
