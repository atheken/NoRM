namespace NoRM
{
    public class MapReduceOptions
    {
        public string Map { get; set; }
        public string Reduce { get; set; }
        public string CollectionName { get; set; }
        public bool Permenant { get; set; }
        public string OutputCollectionName { get; set; }

        public MapReduceOptions(){}
        public MapReduceOptions(string collectionName)
        {
            CollectionName = collectionName;
        }
    }

    public class MapReduceOptions<T> : MapReduceOptions
    {
        public MapReduceOptions()
        {
            CollectionName = typeof (T).Name;
        }

    }
}