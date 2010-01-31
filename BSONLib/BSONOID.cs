using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSONLib
{
    public class BSONOID
    {
        
        /// <summary>
        /// Provides an empty OID (all zeros).
        /// </summary>
        public static BSONOID EMPTY
        {
            get
            {
                return new BSONOID();
            }
        }

        /// <summary>
        /// Generates a new unique oid for use with MongoDB Objects.
        /// </summary>
        /// <returns></returns>
        public static BSONOID NewOID()
        {
            //TODO: generate random-ish bits.
            return new BSONOID();
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
