using Norm.Configuration;

namespace Norm.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// A command to request the databases in a mongoDB instance.
    /// </summary>
    public class ListDatabasesRequest : ISystemQuery
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
        /// Whether or not to list the databases.
        /// </summary>
        /// <value>The ListDatabases property gets the ListDatabases data member.</value>
        public bool ListDatabases
        {
            get { return true; }
        }
    }
}