using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// Describes an index to insert into the db.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    public class MongoIndex<U>
    {
        public U key { get; set; }
        public String ns { get; set; }
        public bool unique { get; set; }
        public String name { get; set; }
    }
}
