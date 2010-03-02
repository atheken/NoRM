namespace NoRM.Protocol.SystemMessages.Responses
{
    public class DatabaseInfo
    {
        public string Name { get; set; }
        public double? SizeOnDisk { get; set; }
        public bool Empty { get; set; }
    }
}
