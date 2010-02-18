using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.BSON.DbTypes
{
    public class OID
    {
        
        /// <summary>
        /// Provides an empty OID (all zeros).
        /// </summary>
        public static OID EMPTY
        {
            get
            {
                return new OID();
            }
        }

        /// <summary>
        /// Generates a new unique oid for use with MongoDB Objects.
        /// </summary>
        /// <returns></returns>
        public static OID NewOID()
        {
            //TODO: generate random-ish bits.
            var n = new OID();
            n.Value = OidGenerator.Generate();
            return n;
        }

        /// <summary>
        /// A 12-byte unique identifier.
        /// </summary>
        public byte[] Value
        {
            get;
            set;
        } 
    }
}
