using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Norm.BSON;

namespace Norm
{
    /// <summary>
    /// Flags a type as having a discriminator.  Apply to a base type to enable multiple-inheritance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class MongoDiscriminatedAttribute : Attribute
    {
        private static readonly Type AttributeType = typeof(MongoDiscriminatedAttribute);
        private static readonly Type RootType = typeof(object);

        private static HybridDictionary DiscriminatorDictionary = new HybridDictionary();

        private static Type GetDiscriminatingTypeForInternal(Type type)
        {
            var current = type;
            while (current != RootType && current != null)
            {
                if (current.IsDefined(AttributeType, false))
                    return current;

                current = current.BaseType;
            }

            foreach (var @interface in type.GetInterfaces())
            {
                if (@interface.IsDefined(AttributeType, false))
                    return @interface;
            }

            return null;
        }

        /// <summary>
        /// Retrieves type discriminators from cache to avoid calling Type.IsDefined(), 
        /// which needs to be called very often (walking the hierarchy) and is quite 
        /// expensive per call.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type GetDiscriminatingTypeForCached(Type type)
        {
            if (DiscriminatorDictionary.Contains(type.GetHashCode()))
                return (Type)(DiscriminatorDictionary[type.GetHashCode()]);
            else
            {
                Type result = GetDiscriminatingTypeForInternal(type);
                DiscriminatorDictionary.Add(type.GetHashCode(), result);
                return result;
            }
        }

        /// <summary>
        /// Finds the sub-type or interface from the given type that declares itself as a discriminating base class
        /// </summary>
        /// <param retval="type"></param>
        /// <returns></returns>
        public static Type GetDiscriminatingTypeFor(Type type)
        {
            return GetDiscriminatingTypeForCached(type);
        }
    }
}