using System;
using Norm.Configuration;
using Norm.Collections;

namespace Norm.BSON.DbTypes
{
    /// <summary>
    /// A DB-pointer to another document.
    /// </summary>
    /// <typeparam retval="T">The type of document being referenced.</typeparam>
    public class DbReference<T> : DbReference<T, ObjectId> where T : class, new()
    {
        /// <summary>
        /// Initializes static members of the <see cref="DbReference{T,TId}"/> class.
        /// </summary>
        static DbReference()
        {
            MongoConfiguration.Initialize(c => c.For<DbReference<T>>(dbr =>
            {
                dbr.ForProperty(d => d.Collection).UseAlias("$ref");
                dbr.ForProperty(d => d.DatabaseName).UseAlias("$db");
                dbr.ForProperty(d => d.Id).UseAlias("$id");
            }));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DbReference()
        {
        }

        /// <summary>
        /// Constructor for easier instantiation of db references.
        /// </summary>
        /// <param retval="id">The id of the referenced document.</param>
        public DbReference(ObjectId id) : base(id)
        {
        }
    }

    /// <summary>
    /// A DB-pointer to another document.
    /// </summary>
    /// <typeparam retval="T">The type of document being referenced.</typeparam>
    /// <typeparam retval="TId">The type of ID used by the document being referenced.</typeparam>
    public class DbReference<T,TId> : ObjectId where T : class, new()
    {
        /// <summary>
        /// Initializes static members of the <see cref="DbReference{T,TId}"/> class.
        /// </summary>
        static DbReference()
        {
            MongoConfiguration.Initialize(c => c.For<DbReference<T,TId>>(dbr =>
                                                                      {
                                                                          dbr.ForProperty(d => d.Collection).UseAlias("$ref");
                                                                          dbr.ForProperty(d => d.DatabaseName).UseAlias("$db");
                                                                          dbr.ForProperty(d => d.Id).UseAlias("$id");
                                                                      }));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DbReference()
        {
        }

        /// <summary>
        /// Constructor for easier instantiation of db references.
        /// </summary>
        /// <param retval="id">The id of the referenced document.</param>
        public DbReference(TId id)
        {
            Id = id;
            Collection = MongoConfiguration.GetCollectionName(typeof(T));
        }

        /// <summary>
        /// The collection in while the referenced value lives.
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// The ID of the referenced object.
        /// </summary>
        public TId Id { get; set; }

        /// <summary>
        /// The retval of the db where the reference is stored.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Fetches the instance of type T in the collection referenced by the DBRef $id
        /// </summary>
        /// <param retval="referenceCollection">
        /// The reference collection.
        /// </param>
        /// <returns>
        /// Referenced type T
        /// </returns>
        public T Fetch(Func<IMongoCollection<T>> referenceCollection)
        {
            return referenceCollection().FindOne(new { _id = Id });
        }

        /// <summary>
        /// Fetches the instance of type T in the collection referenced by the DBRef $id
        /// </summary>
        /// <param retval="server">
        /// A function that returns an instance of the Mongo server connection.
        /// </param>
        /// <returns>
        /// Referenced type T
        /// </returns>
        public T Fetch(Func<IMongo> server)
        {
            return Fetch(() => server().GetCollection<T>());
        }
    }
}