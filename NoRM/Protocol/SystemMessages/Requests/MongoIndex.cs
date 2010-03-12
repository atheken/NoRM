
using NoRM.Configuration;
using NoRM.Protocol.SystemMessages.Request;

namespace NoRM.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// Describes an index to insert into the db.
    /// </summary>
    /// <typeparam name="U">
    /// </typeparam>
    public class MongoIndex<U>
    {
        static MongoIndex()
        {
            MongoConfiguration.Initialize(c =>
                c.For<MongoIndex<U>>(a =>
                                                   {
                                                       a.ForProperty(auth => auth.Key).UseAlias("key");
                                                       a.ForProperty(auth => auth.Namespace).UseAlias("ns");
                                                       a.ForProperty(auth => auth.Unique).UseAlias("unique");
                                                       a.ForProperty(auth => auth.Name).UseAlias("name");
                                                   })
                );
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public U Key { get; set; }

        /// <summary>
        /// Gets or sets the ns.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unique.
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }
}