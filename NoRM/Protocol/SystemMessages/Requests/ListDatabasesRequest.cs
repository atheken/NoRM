namespace NoRM.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// A command to request the databases in a mongoDB instance.
    /// </summary>
    public class ListDatabasesRequest
    {
        /// <summary>
        /// Gets a value indicating whether listDatabases.
        /// </summary>
        public bool listDatabases
        {
            get { return true; }
        }
    }
}