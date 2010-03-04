using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Attributes;
using NoRM.Configuration;

namespace NoRM.BSON.DbTypes
{
    /// <summary>
    /// A DB-pointer to another document.
    /// </summary>
    public class DBReference
    {
        /// <summary>
        /// indicates that the DBReference mapping has happened.
        /// </summary>
        private static bool? _mapped;

        public DBReference()
        {
            if (DBReference._mapped == null)
            {
                //MongoConfiguration.Initialize(config => config.For<DBReference>(cfg =>
                //{
                //    cfg.ForProperty(h => h.Collection).UseAlias("$ref");
                //    cfg.ForProperty(h => h.DatabaseName).UseAlias("$db");
                //    cfg.ForProperty(h => h.ID).UseAlias("$id");
                //}));
                DBReference._mapped = true;
            }
        }

        /// <summary>
        /// The collection in while the referenced value lives.
        /// </summary>
        public String Collection { get; set; }

        /// <summary>
        /// The ID of the referenced object.
        /// </summary>
        public object ID { get; set; }

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
        /// <typeparam name="U"></typeparam>
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
