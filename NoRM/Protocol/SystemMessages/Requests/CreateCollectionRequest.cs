namespace NoRM.Protocol.SystemMessages.Request
{
    internal class CreateCollectionRequest
    {
        private readonly CreateCollectionOptions _options;
        public string create
        {
            get { return _options.Name; }
        }
        public int size
        {
            get { return _options.Size; }
        }
        public long? max
        {
            get { return _options.Max; }
        }
        public bool capped
        {
            get { return _options.Capped; }
        }
        public bool autoIndexId
        {
            get { return _options.AutoIndex;}
        }

        public CreateCollectionRequest(CreateCollectionOptions options)
        {
            _options = options;
        }               
    }
}