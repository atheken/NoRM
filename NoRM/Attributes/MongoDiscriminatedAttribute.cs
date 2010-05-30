using System;
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

        /// <summary>
        /// Finds the sub-type or interface from the given type that declares itself as a discriminating base class
        /// </summary>
        /// <param retval="type"></param>
        /// <returns></returns>
        public static Type GetDiscriminatingTypeFor(Type type)
        {
            var current = type;
            while (current != RootType)
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
    }
}