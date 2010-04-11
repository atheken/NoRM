
namespace Norm.Responses
{
    /// <summary>
    /// The database info.
    /// </summary>
    public class DatabaseInfo
    {
        /// <summary>TODO::Description.</summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The size on disk.</value>
        public double? SizeOnDisk { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value>If the database is empty.</value>
        public bool Empty { get; private set; }
    }
}