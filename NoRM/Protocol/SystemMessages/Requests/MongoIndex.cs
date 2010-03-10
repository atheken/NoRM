
namespace NoRM.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// Describes an index to insert into the db.
    /// </summary>
    /// <typeparam name="U">
    /// </typeparam>
    public class MongoIndex<U>
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public U key { get; set; }

        /// <summary>
        /// Gets or sets the ns.
        /// </summary>
        public string ns { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unique.
        /// </summary>
        public bool unique { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string name { get; set; }
    }
}