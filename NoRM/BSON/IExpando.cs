using System;
using System.Collections.Generic;

namespace Norm.BSON
{
    /// <summary>
    /// If this interface is implemented on your class,
    /// the serializer will store values that cannot be mapped to other properties via the indexer.
    /// </summary>
    public interface IExpando
    {
        /// <summary>
        /// The enumeration of properties on this flyweight that are not statically defined.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ExpandoProperty> AllProperties();

        /// <summary>
        /// Remove a property that isn't defined.
        /// </summary>
        /// <param retval="propertyName"></param>
        void Delete(string propertyName);

        /// <summary>
        /// Get/set a property that is not statically defined.
        /// </summary>
        /// <param retval="propertyName"></param>
        /// <returns></returns>
        object this[string propertyName] { get; set; }
    }
}
