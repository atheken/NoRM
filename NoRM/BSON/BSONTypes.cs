namespace Norm.BSON
{
    /// <summary>
    /// Represents the various  types available from within MongoDB
    /// </summary>
    public enum BSONTypes
    {
        /// <summary>TODO::Description.</summary>
        Double = 1,
        /// <summary>TODO::Description.</summary>
        String = 2,
        /// <summary>TODO::Description.</summary>
        Object = 3,
        /// <summary>TODO::Description.</summary>
        Array = 4,
        /// <summary>TODO::Description.</summary>
        Binary = 5,
        /// <summary>TODO::Description.</summary>
        Undefined = 6,
        /// <summary>TODO::Description.</summary>
        MongoOID = 7,
        /// <summary>TODO::Description.</summary>
        Boolean = 8,
        /// <summary>TODO::Description.</summary>
        DateTime = 9,
        /// <summary>TODO::Description.</summary>
        Null = 10,
        /// <summary>TODO::Description.</summary>
        Regex = 11,
        /// <summary>TODO::Description.</summary>
        Reference = 12,
        /// <summary>TODO::Description.</summary>
        Code = 13,
        /// <summary>TODO::Description.</summary>
        Symbol = 14,
        /// <summary>TODO::Description.</summary>
        ScopedCode = 15,
        /// <summary>TODO::Description.</summary>
        Int32 = 16,
        /// <summary>TODO::Description.</summary>
        Timestamp = 17,
        /// <summary>TODO::Description.</summary>
        Int64 = 18,
        // MinKey = -1,
        // MaxKey = 127
    }
}