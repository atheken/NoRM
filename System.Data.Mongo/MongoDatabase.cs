using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Mongo.Protocol.SystemMessages.Responses;

namespace System.Data.Mongo
{
    public class MongoDatabase
    {
        private String _dbName;
        private MongoContext _context;
        /// <summary>
        /// A reference to the database found using the specified context.
        /// </summary>
        /// <param name="dbname"></param>
        /// <param name="context"></param>
        public MongoDatabase(String dbname, MongoContext context)
        {
            this._dbName = dbname;
            this._context = context;
            this.SizeOnDisk = 0.0;
        }

        public Double SizeOnDisk { get; set; }

        /// <summary>
        /// The database name for this database.
        /// </summary>
        public String DatabaseName
        {
            get
            {
                return this._dbName;
            }
        }


        /// <summary>
        /// Produces a mongodb collection that will produce and
        /// manipulate objects of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public MongoCollection<T> GetCollection<T>(string collectionName) where T : class, new()
        {
            var retval = new MongoCollection<T>(collectionName, this, this._context);

            return retval;
        }


        /// <summary>
        /// Produces a list of all collections currently in this database.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CollectionInfo> GetAllCollections()
        {
            var results = this.GetCollection<CollectionInfo>("system.namespaces").Find();                

            return results;
        }
    }
}
