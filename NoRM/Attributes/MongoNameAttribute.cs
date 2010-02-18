using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Attributes
{
    /// <summary>
    /// Provides a way to specify a different name when de/serialized to and from MongoDB.
    /// </summary>
    /// <remarks>
    /// BSONSerializer doesn't check for these (YET!)
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class MongoNameAttribute :Attribute
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameToUseInTheDatabase">The property name in the database.</param>
        public MongoNameAttribute(String nameToUseInTheDatabase)
        {
            this.Name = nameToUseInTheDatabase;
        }

        /// <summary>
        /// The name for this property in the database.
        /// </summary>
        public String Name
        {
            protected set;
            get;
        }
    }
}
