using System;
using System.Collections.Generic;

namespace NoRM.BSON
{
    /// <summary>
    /// The bson helper.
    /// </summary>
    internal class BsonHelper
    {
        /// <summary>
        /// The cod e_ length.
        /// </summary>
        public const int CODE_LENGTH = 1;

        /// <summary>
        /// The ke y_ terminato r_ length.
        /// </summary>
        public const int KEY_TERMINATOR_LENGTH = 1;

        /// <summary>
        /// The epoch.
        /// </summary>
        public static readonly DateTime EPOCH = new DateTime(1970, 1, 1).ToUniversalTime();

        /// <summary>
        /// The mongo identifier attribute.
        /// </summary>
        public static readonly Type MongoIdentifierAttribute = typeof(MongoIdentifierAttribute);

        /// <summary>
        /// The prohibitted types.
        /// </summary>
        public static readonly HashSet<Type> ProhibittedTypes = new HashSet<Type>();
    }
}