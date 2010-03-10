
namespace NoRM.Responses
{
    /// <summary>
    /// The database info.
    /// </summary>
    public class DatabaseInfo
    {
        public string Name { get; set; }
        public double? SizeOnDisk { get; set; }
        public bool Empty { get; set; }
    }
}