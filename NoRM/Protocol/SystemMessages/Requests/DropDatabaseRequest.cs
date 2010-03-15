using NoRM.Configuration;

namespace NoRM.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// The drop database request.
    /// </summary>
    internal class DropDatabaseRequest : ISystemQuery
    {
        /// <summary>
        /// Initializes the <see cref="DropDatabaseRequest"/> class.
        /// </summary>
        static DropDatabaseRequest()
        {
            MongoConfiguration.Initialize(c =>
                c.For<DropDatabaseRequest>(a => a.ForProperty(auth => auth.DropDatabase).UseAlias("dropDatabase")));
        }
        /// <summary>
        /// Drop database.
        /// </summary>
        public double DropDatabase
        {
            get { return 1d; }
        }
    }
}