namespace NoRM.BSON
{
    using System;
    using System.Collections.Generic;
    
    internal class BsonHelper
    {
        public static readonly Type MongoIdentifierAttribute = typeof(MongoIdentifierAttribute);
        public static readonly DateTime EPOCH = new DateTime(1970, 1, 1).ToUniversalTime();
        public const int CODE_LENGTH = 1;
        public const int KEY_TERMINATOR_LENGTH = 1;
        
        public readonly static HashSet<Type> ProhibittedTypes = new HashSet<Type>();
    }
}