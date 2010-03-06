using System;
using NoRM.Configuration;

namespace NoRM.BSON.DbTypes
{
    /// <summary>
    /// A DB-pointer to another document.
    /// </summary>
    public class DBReference : ObjectId
    {
        static DBReference()
        {
            MongoConfiguration.Initialize(c => c.For<DBReference>(dbr =>
                                                                      {
                                                                          dbr.ForProperty(d => d.Collection).UseAlias("$ref");
                                                                          dbr.ForProperty(d => d.DatabaseName).UseAlias("$db");
                                                                          dbr.ForProperty(d => d.ID).UseAlias("$id");
                                                                      }));
        }

        /// <summary>
        /// The collection in while the referenced value lives.
        /// </summary>
        public String Collection { get; set; }

        /// <summary>
        /// The ID of the referenced object.
        /// </summary>
        public ObjectId ID { get; set; }

        /// <summary>
        /// The name of the db where the reference is stored.
        /// </summary>
        public String DatabaseName { get; set; }

        /// <summary>
        /// Pulls the document using the connection specified.
        /// </summary>
        /// <remarks>
        /// This is not quite right yet....
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        public virtual T FollowReference<T>(Mongo connection) where T : class, new()
        {

            return connection.GetCollection<T>().FindOne(new { _id = this.ID });
        }
    }

    /// <summary>
    /// Represents a DB-pointer to another BSON document.
    /// </summary>
    /// <remarks>This is purely a convenience so that we don't have to specify
    /// the type each time FollowReference</remarks>
    public class DBReference<U>  : DBReference where U : class, new()
    {
        public override U FollowReference<U>(Mongo connection)
        {
            return base.FollowReference<U>(connection);
        }
        
    }
}
