using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.BSON
{
    /// <summary>
    /// Represents the various 
    /// types available from within
    /// MongoDB
    /// </summary>
    public enum BSONTypes
    {
        Double  = 1,
        String = 2,
        Object = 3,
        Array = 4,
        Binary = 5,
        Undefined = 6,
        MongoOID = 7,
        Boolean = 8,
        DateTime = 9,
        Null = 10,
        Regex = 11,
        Reference = 12,
        Code = 13,
        Symbol = 14,
        ScopedCode = 15,
        Int32 = 16,
        Timestamp = 17,
        Int64 = 18,
        //MinKey = -1,
        //MaxKey = 127
    }
}
