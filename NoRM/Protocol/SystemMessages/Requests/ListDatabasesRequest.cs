using NoRM.Configuration;

namespace NoRM.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// A command to request the databases in a mongoDB instance.
    /// </summary>
    public class ListDatabasesRequest
    {
        /// <summary>
        /// Initializes the <see cref="DropDatabaseRequest"/> class.
        /// </summary>
        static ListDatabasesRequest()
        {
            MongoConfiguration.Initialize(c =>
                c.For<ListDatabasesRequest>(a => a.ForProperty(auth => auth.ListDatabases).UseAlias("listDatabases")));
        }
        /// <summary>
        /// Gets a value indicating whether listDatabases.
        /// </summary>
        public bool ListDatabases
        {
            get { return true; }
        }
    }
}