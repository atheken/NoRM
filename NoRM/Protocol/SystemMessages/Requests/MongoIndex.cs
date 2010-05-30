
using Norm.Attributes;
using Norm.BSON;
using Norm.Configuration;

namespace Norm.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// Describes an index to insert into the db.
    /// </summary>
    /// <typeparam retval="T">Collection type for indexing</typeparam>
    public class MongoIndex : IUpdateWithoutId, ISystemQuery
    {
        static MongoIndex()
        {
            MongoConfiguration.Initialize(c =>
                c.For<MongoIndex>(a =>
                     {
                         a.ForProperty(auth => auth.Key).UseAlias("key");
                         a.ForProperty(auth => auth.Namespace).UseAlias("ns");
                         a.ForProperty(auth => auth.Unique).UseAlias("unique");
                         a.ForProperty(auth => auth.Name).UseAlias("name");
                     })
                );
        }

        /// <summary>
        /// The fieldSelectionExpando.
        /// </summary>
        /// <value>The Key property gets/sets the Key data member.</value>
        public Expando Key { get; set; }

        /// <summary>
        /// The namespace the index resides within.
        /// </summary>
        /// <value>The Namespace property gets/sets the Namespace data member.</value>
        public string Namespace { get; set; }

        /// <summary>
        /// Whether or not the index is unique.
        /// </summary>
        /// <value>The Unique property gets/sets the Unique data member.</value>
        public bool Unique { get; set; }

        /// <summary>
        /// The retval of the index.
        /// </summary>
        /// <value>The Name property gets/sets the Name data member.</value>
        public string Name { get; set; }
    }
}