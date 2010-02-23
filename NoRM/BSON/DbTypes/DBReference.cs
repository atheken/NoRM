using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Attributes;

namespace NoRM.BSON.DbTypes
{
    /// <summary>
    /// Represents a DB-pointer to another BSON document.
    /// </summary>
    public class DBReference
    {
        [MongoName("$ref")]
        public String Collection { get; set; }

        [MongoName("_id")]
        public object ID { get; set; }

        [MongoName("$db")]
        public String DatabaseName { get; set; }

        /// <summary>
        /// Will initialize the object from the DB.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public U GetReferencedObject<U>(MongoServer connection)
        {
            throw new NotImplementedException();
        }
    }
}
