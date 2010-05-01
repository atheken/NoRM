using System;
using System.Collections.Generic;

namespace Norm.BSON
{
    /// <summary>
    /// The bson helper.
    /// </summary>
    internal class BsonHelper
    {
        public const int CODE_LENGTH = 1;
        public const int KEY_TERMINATOR_LENGTH = 1;
        public static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly Type MongoIdentifierAttribute = typeof(MongoIdentifierAttribute);
        public static readonly Type MongoDiscriminatedAttribute = typeof(MongoDiscriminatedAttribute);
        public static readonly HashSet<Type> ProhibittedTypes = new HashSet<Type>();
    }
}