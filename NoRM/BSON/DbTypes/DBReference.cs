using System;
using Norm.Configuration;
using Norm.Collections;

namespace Norm.BSON.DbTypes
{
    /// <summary>
    /// A DB-pointer to another document.
    /// </summary>
	public class DBReference<T> : ObjectId where T : class, new()
    {
        /// <summary>
        /// Initializes static members of the <see cref="DBReference"/> class.
        /// </summary>
        static DBReference()
        {
            MongoConfiguration.Initialize(c => c.For<DBReference<T>>(dbr =>
                                                                      {
                                                                          dbr.ForProperty(d => d.Collection).UseAlias("$ref");
                                                                          dbr.ForProperty(d => d.DatabaseName).UseAlias("$db");
                                                                          dbr.ForProperty(d => d.ID).UseAlias("$id");
                                                                      }));
        }

        /// <summary>
        /// The collection in while the referenced value lives.
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// The ID of the referenced object.
        /// </summary>
        public ObjectId ID { get; set; }

        /// <summary>
        /// The name of the db where the reference is stored.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Fetches the instance of type T in the collection referenced by the DBRef $id
        /// </summary>
        /// <typeparam name="T">
        /// Type referenced by DBRef
        /// </typeparam>
        /// <param name="referenceCollection">
        /// The reference collection.
        /// </param>
        /// <returns>
        /// Referenced type T
        /// </returns>
        public T Fetch(Func<MongoCollection<T>> referenceCollection)
        {
            return referenceCollection().FindOne(new { _id = this.ID });
        }

		/// <summary>
		/// Fetches the instance of type T in the collection referenced by the DBRef $id
		/// </summary>
		/// <typeparam name="T">
		/// Type referenced by DBRef
		/// </typeparam>
		/// <param name="server">
		/// A function that returns an instance of the Mongo server connection.
		/// </param>
		/// <returns>
		/// Referenced type T
		/// </returns>
		public T Fetch(Func<Mongo> server)
		{
			return Fetch(() => server().GetCollection<T>());
		}
    }
}